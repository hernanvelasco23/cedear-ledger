using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class IolAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly IolOptions _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string? _token;
    private DateTimeOffset _expiresAt;

    public IolAuthClient(HttpClient httpClient, IOptions<IolOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> EnsureTokenAsync(CancellationToken cancellationToken)
    {
        if (IsTokenValid())
        {
            return _token!;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (IsTokenValid())
            {
                return _token!;
            }

            await FetchTokenAsync(cancellationToken);
            return _token!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<string> RefreshTokenAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await FetchTokenAsync(cancellationToken);
            return _token!;
        }
        finally
        {
            _lock.Release();
        }
    }

    private bool IsTokenValid()
    {
        return !string.IsNullOrWhiteSpace(_token) && _expiresAt > DateTimeOffset.UtcNow;
    }

    private async Task FetchTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
        {
            throw new InvalidOperationException("IOL credentials are not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "/token");
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = _options.Username,
            ["password"] = _options.Password,
            ["grant_type"] = "password"
        });
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var trimmed = string.IsNullOrWhiteSpace(body) ? string.Empty : body.Trim();
            if (trimmed.Length > 200)
            {
                trimmed = trimmed.Substring(0, 200);
            }
            var detail = string.IsNullOrWhiteSpace(trimmed) ? string.Empty : $" Body: {trimmed}";
            throw new InvalidOperationException($"IOL token request failed: {response.StatusCode}.{detail}");
        }

        var payload = await JsonSerializer.DeserializeAsync<TokenResponse>(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        if (payload is null || string.IsNullOrWhiteSpace(payload.AccessToken))
        {
            throw new InvalidOperationException("IOL token response missing access_token.");
        }

        var expiresIn = payload.ExpiresIn <= 0 ? 3600 : payload.ExpiresIn;
        _token = payload.AccessToken;
        var refreshIn = Math.Max(60, expiresIn - 60);
        _expiresAt = DateTimeOffset.UtcNow.AddSeconds(refreshIn);
    }

    private sealed class TokenResponse
    {
        public string? AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string? access_token { get => AccessToken; set => AccessToken = value; }
        public int expires_in { get => ExpiresIn; set => ExpiresIn = value; }
    }
}
