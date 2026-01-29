using System.Net;
using System.Net.Http;
using System.Text;
using CedearLedger.Infrastructure.Persistence.SqlServer;
using Microsoft.Extensions.Options;
using Xunit;

namespace CedearLedger.Tests.Ingestion;

public sealed class IolMarketDataProviderTests
{
    [Fact]
    public async Task Token_Is_Requested_And_Quote_Parsed()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath == "/token")
            {
                var json = "{\"access_token\":\"token1\",\"expires_in\":3600}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            var quoteJson = "{\"ultimoPrecio\":1234.5,\"moneda\":\"ARS\",\"montoOperado\":100}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(quoteJson, Encoding.UTF8, "application/json")
            };
        });

        var provider = CreateProvider(handler);

        var result = await provider.GetDollarRatesAsync(new DateOnly(2026, 1, 28), CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 1, 28), result.Date);
        Assert.Equal("IOL", result.Mep.Source);
        Assert.True(result.Mep.Rate > 0);
    }

    [Fact]
    public async Task Quote_401_Triggers_Token_Refresh()
    {
        var tokenCalls = 0;
        var quoteCalls = 0;

        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath == "/token")
            {
                tokenCalls++;
                var json = "{\"access_token\":\"token" + tokenCalls + "\",\"expires_in\":3600}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            quoteCalls++;
            if (quoteCalls == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }

            var quoteJson = "{\"ultimo\":10,\"moneda\":\"USD\"}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(quoteJson, Encoding.UTF8, "application/json")
            };
        });

        var provider = CreateProvider(handler);

        await provider.GetCedearPricesAsync(new DateOnly(2026, 1, 28), new[] { "AAPL" }, CancellationToken.None);

        Assert.Equal(2, tokenCalls);
        Assert.Equal(2, quoteCalls);
    }

    [Fact]
    public async Task Parses_Price_Volume_And_Currency()
    {
        var handler = new FakeHttpMessageHandler(request =>
        {
            if (request.RequestUri!.AbsolutePath == "/token")
            {
                var json = "{\"access_token\":\"token1\",\"expires_in\":3600}";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            var quoteJson = "{\"precioUltimo\":4321.0,\"montoOperado\":999.5,\"moneda\":\"USD\"}";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(quoteJson, Encoding.UTF8, "application/json")
            };
        });

        var provider = CreateProvider(handler);
        var quote = await provider.GetQuoteAsync("AL30D", CancellationToken.None);

        Assert.Equal(4321.0m, quote.Price);
        Assert.Equal(999.5m, quote.Volume);
        Assert.Equal("USD", quote.Currency);
    }

    private static IolMarketDataProvider CreateProvider(HttpMessageHandler handler)
    {
        var options = Options.Create(new IolOptions
        {
            BaseUrl = "https://api.invertironline.com",
            Username = "user",
            Password = "pass",
            Mercado = "BCBA",
            Panel = "General"
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(options.Value.BaseUrl)
        };

        var authClient = new IolAuthClient(client, options);
        return new IolMarketDataProvider(client, authClient, options);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
