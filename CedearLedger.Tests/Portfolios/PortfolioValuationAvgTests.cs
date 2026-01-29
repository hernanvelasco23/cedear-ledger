using CedearLedger.Infrastructure.Persistence.SqlServer;
using Xunit;

namespace CedearLedger.Tests.Portfolios;

public sealed class PortfolioValuationAvgTests
{
    [Fact]
    public void Calculates_AvgCost_And_Pnl_For_Multiple_Buys()
    {
        var portfolioId = Guid.NewGuid();
        var aggregates = new[]
        {
            new PortfolioValuationQueryService.OperationAggregate(
                "AAPL",
                BuyQuantity: 15m,
                SellQuantity: 0m,
                BuyCost: 1600m,
                SellCost: 0m)
        };

        var prices = new Dictionary<string, PortfolioValuationQueryService.PriceSnapshot>(StringComparer.OrdinalIgnoreCase)
        {
            ["AAPL"] = new PortfolioValuationQueryService.PriceSnapshot(150m, new DateOnly(2026, 1, 28))
        };

        var result = PortfolioValuationQueryService.ComputeValuation(
            portfolioId,
            aggregates,
            prices,
            new DateTime(2026, 1, 28, 0, 0, 0, DateTimeKind.Utc));

        var position = Assert.Single(result.Positions);
        Assert.Equal(15m, position.Quantity);
        Assert.Equal(1600m, position.CostArs);
        Assert.Equal(150m, position.MarketPriceArs);
        Assert.Equal(2250m, position.MarketValueArs);
        Assert.Equal(1600m / 15m, position.AvgCostArs, 6);
        Assert.Equal((150m - (1600m / 15m)) * 15m, position.UnrealizedPnLArs, 6);
        Assert.Equal((150m / (1600m / 15m)) - 1m, position.UnrealizedPnLPercent, 6);
    }

    [Fact]
    public void Calculates_AvgCost_With_Sells_And_Excludes_Non_Positive()
    {
        var portfolioId = Guid.NewGuid();
        var aggregates = new[]
        {
            new PortfolioValuationQueryService.OperationAggregate(
                "MSFT",
                BuyQuantity: 15m,
                SellQuantity: 4m,
                BuyCost: 1600m,
                SellCost: 520m)
        };

        var prices = new Dictionary<string, PortfolioValuationQueryService.PriceSnapshot>(StringComparer.OrdinalIgnoreCase)
        {
            ["MSFT"] = new PortfolioValuationQueryService.PriceSnapshot(110m, new DateOnly(2026, 1, 28))
        };

        var result = PortfolioValuationQueryService.ComputeValuation(
            portfolioId,
            aggregates,
            prices,
            new DateTime(2026, 1, 28, 0, 0, 0, DateTimeKind.Utc));

        var position = Assert.Single(result.Positions);
        Assert.Equal(11m, position.Quantity);
        Assert.Equal(1080m, position.CostArs);
        Assert.Equal(110m, position.MarketPriceArs);
        Assert.Equal(1210m, position.MarketValueArs);
        Assert.Equal(1080m / 11m, position.AvgCostArs, 6);
        Assert.Equal((110m - (1080m / 11m)) * 11m, position.UnrealizedPnLArs, 6);
    }
}
