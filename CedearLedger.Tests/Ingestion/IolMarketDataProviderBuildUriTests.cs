using System.Net.Http;
using CedearLedger.Infrastructure.Persistence.SqlServer;
using Microsoft.Extensions.Options;
using Xunit;

namespace CedearLedger.Tests.Ingestion;

public sealed class IolMarketDataProviderBuildUriTests
{
    [Fact]
    public void BuildUri_Uses_Relative_Path_With_BaseAddress()
    {
        var provider = CreateProvider("https://api.invertironline.com");
        var uri = provider.BuildUri("/api/v2/Cotizaciones/bonos/AL30?mercado=BCBA");

        Assert.Equal("https://api.invertironline.com/api/v2/Cotizaciones/bonos/AL30?mercado=BCBA", uri.ToString());
    }

    [Fact]
    public void BuildUri_Uses_Absolute_Path_When_Provided()
    {
        var provider = CreateProvider("https://api.invertironline.com");
        var uri = provider.BuildUri("https://data.example.com/api/v2/Cotizaciones/bonos/AL30");

        Assert.Equal("https://data.example.com/api/v2/Cotizaciones/bonos/AL30", uri.ToString());
    }

    private static IolMarketDataProvider CreateProvider(string baseUrl)
    {
        var options = Options.Create(new IolOptions
        {
            BaseUrl = baseUrl,
            Username = "user",
            Password = "pass",
            Mercado = "BCBA",
            Panel = "General"
        });

        var client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };

        var authClient = new IolAuthClient(client, options);
        return new IolMarketDataProvider(client, authClient, options);
    }

}
