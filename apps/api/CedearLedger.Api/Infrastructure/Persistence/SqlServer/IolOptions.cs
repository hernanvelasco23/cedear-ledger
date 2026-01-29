namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class IolOptions
{
    public string BaseUrl { get; set; } = "https://api.invertironline.com";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Mercado { get; set; } = "BCBA";
    public string Panel { get; set; } = "General";
}
