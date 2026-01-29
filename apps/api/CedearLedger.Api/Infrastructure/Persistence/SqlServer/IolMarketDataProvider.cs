using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using CedearLedger.Application.Abstractions;
using CedearLedger.Application.Ingestion;
using Microsoft.Extensions.Options;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class IolMarketDataProvider : IMarketDataProvider
{
    private static readonly string[] PriceFields = { "ultimoPrecio", "precioUltimo", "precio_ultimo", "ultimo" };
    private static readonly string[] VolumeFields = { "montoOperado", "volumen", "montoOperadoEnMoneda", "cantidadOperaciones" };
    private static readonly string[] CurrencyFields = { "moneda", "currency" };
    private static readonly string[] ContainerFields = { "titulo", "cotizacion", "data" };

    private readonly HttpClient _httpClient;
    private readonly IolAuthClient _authClient;
    private readonly IolOptions _options;

    public IolMarketDataProvider(HttpClient httpClient, IolAuthClient authClient, IOptions<IolOptions> options)
    {
        _httpClient = httpClient;
        _authClient = authClient;
        _options = options.Value;
    }

    public async Task<ExternalDollarRatesResult> GetDollarRatesAsync(DateOnly date, CancellationToken cancellationToken)
    {
        decimal mep = 0m;
        decimal ccl = 0m;
        Exception? mepError = null;
        Exception? cclError = null;

        try
        {
            var al30 = await GetQuoteAsync("AL30", cancellationToken);
            var al30d = await GetQuoteAsync("AL30D", cancellationToken);

            if (al30.Price <= 0 || al30d.Price <= 0)
            {
                throw new InvalidOperationException("Invalid AL30/AL30D prices for MEP calculation.");
            }

            mep = al30.Price / al30d.Price;
        }
        catch (Exception ex)
        {
            mepError = ex;
        }

        try
        {
            var gd30 = await GetQuoteAsync("GD30", cancellationToken);
            var gd30d = await GetQuoteAsync("GD30D", cancellationToken);
            if (gd30.Price <= 0 || gd30d.Price <= 0)
            {
                throw new InvalidOperationException("Invalid GD30/GD30D prices for CCL calculation.");
            }
            ccl = gd30.Price / gd30d.Price;
        }
        catch (Exception ex)
        {
            cclError = ex;
        }

        if (mepError is not null)
        {
            if (cclError is not null)
            {
                throw new InvalidOperationException(
                    $"IOL dollar rate calculation failed. MEP error: {mepError.Message}. CCL error: {cclError.Message}.",
                    mepError);
            }

            throw mepError;
        }

        var mepRate = new ExternalDollarRate(mep, "IOL");
        var cclRate = cclError is null ? new ExternalDollarRate(ccl, "IOL") : new ExternalDollarRate(mep, "IOL");

        return new ExternalDollarRatesResult(date, mepRate, cclRate);
    }

    public async Task<ExternalCedearPricesResult> GetCedearPricesAsync(DateOnly date, IReadOnlyList<string> tickers, CancellationToken cancellationToken)
    {
        var prices = new List<ExternalCedearPrice>();

        foreach (var ticker in tickers)
        {
            if (string.IsNullOrWhiteSpace(ticker))
            {
                continue;
            }

            var quote = await GetQuoteAsync(ticker, cancellationToken);
            prices.Add(new ExternalCedearPrice(ticker, quote.Price, "IOL", quote.Currency));
        }

        return new ExternalCedearPricesResult(date, prices);
    }

    internal async Task<QuoteResult> GetQuoteAsync(string symbol, CancellationToken cancellationToken)
    {
        var paths = new[]
        {
            $"/api/v2/Cotizaciones/bonos/{symbol}?mercado={_options.Mercado}&panel={_options.Panel}",
            $"/api/v2/Cotizaciones/Bonos/{symbol}?mercado={_options.Mercado}&panel={_options.Panel}",
            $"/api/v2/Cotizaciones/{_options.Mercado}/bonos/{symbol}",
            "/api/v2/Cotizaciones/argentina/Bonos/" + symbol,
            $"/api/v2/Cotizaciones/Bonos/{symbol}?mercado=Argentina"
        };

        Exception? lastError = null;
        var statusErrors = new List<string>();

        foreach (var path in paths)
        {
            var response = await SendWithAuthRetryAsync(path, statusErrors, cancellationToken);
            if (response is null)
            {
                continue;
            }

            try
            {
                var payload = await JsonSerializer.DeserializeAsync<JsonElement>(
                    await response.Content.ReadAsStreamAsync(cancellationToken),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                    cancellationToken);

                if (!TryExtractDecimal(payload, PriceFields, out var price))
                {
                    lastError = new InvalidOperationException("Price not found in response.");
                    continue;
                }

                TryExtractDecimal(payload, VolumeFields, out var volume);
                TryExtractString(payload, CurrencyFields, out var currency);

                if (string.IsNullOrWhiteSpace(currency))
                {
                    currency = symbol.EndsWith("D", StringComparison.OrdinalIgnoreCase) ? "USD" : "ARS";
                }

                return new QuoteResult(price, volume, currency);
            }
            catch (Exception ex)
            {
                lastError = ex;
            }
            finally
            {
                response.Dispose();
            }
        }

        var statusSummary = statusErrors.Count > 0
            ? " Errors: " + string.Join("; ", statusErrors.Take(3))
            : string.Empty;

        if (lastError is not null)
        {
            throw new InvalidOperationException($"All IOL quote endpoints failed. Last error: {lastError.Message}.{statusSummary}", lastError);
        }

        throw new InvalidOperationException($"All IOL quote endpoints failed.{statusSummary}");
    }

    private async Task<HttpResponseMessage?> SendWithAuthRetryAsync(string path, List<string> errors, CancellationToken cancellationToken)
    {
        var token = await _authClient.EnsureTokenAsync(cancellationToken);
        var response = await SendAsync(path, token, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            token = await _authClient.RefreshTokenAsync(cancellationToken);
            response = await SendAsync(path, token, cancellationToken);
        }

        if (!response.IsSuccessStatusCode)
        {
            errors.Add($"IOL quote failed ({(int)response.StatusCode}): {path}");
            response.Dispose();
            return null;
        }

        return response;
    }

    private Task<HttpResponseMessage> SendAsync(string path, string token, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUri(path));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _httpClient.SendAsync(request, cancellationToken);
    }

    internal Uri BuildUri(string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
        {
            return absolute;
        }

        if (_httpClient.BaseAddress is null)
        {
            throw new InvalidOperationException("IOL BaseUrl is not configured.");
        }

        if (!Uri.TryCreate(_httpClient.BaseAddress, path, out var combined))
        {
            throw new InvalidOperationException("IOL path is invalid.");
        }

        return combined;
    }

    private static bool TryExtractDecimal(JsonElement element, string[] names, out decimal value)
    {
        foreach (var name in names)
        {
            if (TryFindProperty(element, name, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out value))
                {
                    return true;
                }

                if (property.ValueKind == JsonValueKind.String &&
                    decimal.TryParse(property.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
            }
        }

        value = 0m;
        return false;
    }

    private static bool TryExtractString(JsonElement element, string[] names, out string? value)
    {
        foreach (var name in names)
        {
            if (TryFindProperty(element, name, out var property) && property.ValueKind == JsonValueKind.String)
            {
                value = property.GetString();
                return true;
            }
        }

        value = null;
        return false;
    }

    private static bool TryFindProperty(JsonElement element, string propertyName, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (TryGetPropertyIgnoreCase(element, propertyName, out value))
            {
                return true;
            }

            foreach (var container in ContainerFields)
            {
                if (TryGetPropertyIgnoreCase(element, container, out var containerValue) &&
                    TryFindProperty(containerValue, propertyName, out value))
                {
                    return true;
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }

                if (TryFindProperty(property.Value, propertyName, out value))
                {
                    return true;
                }
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (TryFindProperty(item, propertyName, out value))
                {
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    internal sealed record QuoteResult(decimal Price, decimal Volume, string Currency);
}
