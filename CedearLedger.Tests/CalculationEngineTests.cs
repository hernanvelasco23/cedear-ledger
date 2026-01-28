using System;
using System.Collections.Generic;
using CedearLedger.Domain.Models;
using CedearLedger.Domain.Services;
using Xunit;

namespace CedearLedger.Tests;

public sealed class CalculationEngineTests
{
    [Fact]
    public void Uses_Sell_Rate_Only_For_Current_Fx()
    {
        var tradeDate = new DateOnly(2024, 1, 10);
        var operation = CreateOperation(tradeDate, "AAPL", 10m, 100m, 0m);
        var currentPrice = CreatePrice("AAPL", 120m);

        var currentMep = CreateFx(FxRateType.Mep, new DateOnly(2024, 1, 10), 100m, isSellRate: false);
        var currentCcl = CreateFx(FxRateType.Ccl, new DateOnly(2024, 1, 10), 200m, isSellRate: true);

        var historicalMep = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Mep, tradeDate, 100m, isSellRate: true)
        };
        var historicalCcl = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Ccl, tradeDate, 200m, isSellRate: true)
        };

        var request = BuildRequest(
            new[] { operation },
            new Dictionary<string, CedearPriceRecord> { ["AAPL"] = currentPrice },
            currentMep,
            currentCcl,
            historicalMep,
            historicalCcl);

        var result = new CalculationEngine().Calculate(request);
        var summary = Assert.Single(result.Tickers);

        Assert.Null(summary.CurrentValuation.CurrentValueUsdMep);
        Assert.NotNull(summary.CurrentValuation.CurrentValueUsdCcl);
        Assert.False(summary.CurrentValuation.IsComplete);
    }

    [Fact]
    public void Historical_Fx_Fallback_Uses_Closest_Previous_Available_Rate()
    {
        var tradeDate = new DateOnly(2024, 1, 10);
        var operation = CreateOperation(tradeDate, "MSFT", 5m, 100m, 0m);

        var historicalMep = new Dictionary<DateOnly, FxRateRecord>
        {
            [new DateOnly(2024, 1, 5)] = CreateFx(FxRateType.Mep, new DateOnly(2024, 1, 5), 90m, isSellRate: true),
            [new DateOnly(2024, 1, 8)] = CreateFx(FxRateType.Mep, new DateOnly(2024, 1, 8), 100m, isSellRate: true)
        };
        var historicalCcl = new Dictionary<DateOnly, FxRateRecord>
        {
            [new DateOnly(2024, 1, 5)] = CreateFx(FxRateType.Ccl, new DateOnly(2024, 1, 5), 180m, isSellRate: true),
            [new DateOnly(2024, 1, 8)] = CreateFx(FxRateType.Ccl, new DateOnly(2024, 1, 8), 200m, isSellRate: true)
        };

        var request = BuildRequest(
            new[] { operation },
            new Dictionary<string, CedearPriceRecord>(),
            currentMep: null,
            currentCcl: null,
            historicalMep,
            historicalCcl);

        var result = new CalculationEngine().Calculate(request);
        var cost = Assert.Single(result.Tickers).OperationCosts[0];

        Assert.Equal(new DateOnly(2024, 1, 8), cost.UsdCostMep!.FxRate.RateDate);
        Assert.Equal(new DateOnly(2024, 1, 8), cost.UsdCostCcl!.FxRate.RateDate);
    }

    [Fact]
    public void Missing_Historical_Fx_Marks_Incomplete()
    {
        var tradeDate = new DateOnly(2024, 1, 10);
        var operation = CreateOperation(tradeDate, "AMZN", 2m, 100m, 0m);

        var historicalMep = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Mep, tradeDate, 100m, isSellRate: true)
        };
        var historicalCcl = new Dictionary<DateOnly, FxRateRecord>();

        var request = BuildRequest(
            new[] { operation },
            new Dictionary<string, CedearPriceRecord>(),
            currentMep: null,
            currentCcl: null,
            historicalMep,
            historicalCcl);

        var result = new CalculationEngine().Calculate(request);
        var summary = Assert.Single(result.Tickers);

        Assert.False(summary.OperationCosts[0].IsComplete);
        Assert.False(summary.Totals.IsComplete);
        Assert.False(summary.ProfitAndLoss.IsComplete);
    }

    [Fact]
    public void Missing_Cedear_Price_Marks_Incomplete()
    {
        var tradeDate = new DateOnly(2024, 1, 10);
        var operation = CreateOperation(tradeDate, "TSLA", 1m, 100m, 0m);

        var historicalMep = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Mep, tradeDate, 100m, isSellRate: true)
        };
        var historicalCcl = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Ccl, tradeDate, 200m, isSellRate: true)
        };

        var request = BuildRequest(
            new[] { operation },
            new Dictionary<string, CedearPriceRecord>(),
            currentMep: CreateFx(FxRateType.Mep, tradeDate, 100m, isSellRate: true),
            currentCcl: CreateFx(FxRateType.Ccl, tradeDate, 200m, isSellRate: true),
            historicalMep,
            historicalCcl);

        var result = new CalculationEngine().Calculate(request);
        var summary = Assert.Single(result.Tickers);

        Assert.Null(summary.CurrentValuation.Price);
        Assert.False(summary.CurrentValuation.IsComplete);
    }

    [Fact]
    public void Mep_And_Ccl_Are_Independent()
    {
        var tradeDate = new DateOnly(2024, 1, 10);
        var operation = CreateOperation(tradeDate, "NVDA", 1m, 1000m, 0m);

        var historicalMep = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Mep, tradeDate, 100m, isSellRate: true)
        };
        var historicalCcl = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Ccl, tradeDate, 200m, isSellRate: true)
        };

        var request = BuildRequest(
            new[] { operation },
            new Dictionary<string, CedearPriceRecord>(),
            currentMep: null,
            currentCcl: null,
            historicalMep,
            historicalCcl);

        var result = new CalculationEngine().Calculate(request);
        var totals = Assert.Single(result.Tickers).Totals;

        Assert.Equal(10m, totals.TotalInvestedUsdMep!.Value);
        Assert.Equal(5m, totals.TotalInvestedUsdCcl!.Value);
        Assert.Equal(FxRateType.Mep, totals.TotalInvestedUsdMep.FxType);
        Assert.Equal(FxRateType.Ccl, totals.TotalInvestedUsdCcl.FxType);
    }

    [Fact]
    public void Output_Includes_Fx_Rate_Value_And_Date()
    {
        var tradeDate = new DateOnly(2024, 1, 10);
        var operation = CreateOperation(tradeDate, "META", 3m, 100m, 0m);
        var currentPrice = CreatePrice("META", 110m);

        var currentMep = CreateFx(FxRateType.Mep, new DateOnly(2024, 1, 12), 120m, isSellRate: true);
        var currentCcl = CreateFx(FxRateType.Ccl, new DateOnly(2024, 1, 12), 240m, isSellRate: true);

        var historicalMep = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Mep, tradeDate, 100m, isSellRate: true)
        };
        var historicalCcl = new Dictionary<DateOnly, FxRateRecord>
        {
            [tradeDate] = CreateFx(FxRateType.Ccl, tradeDate, 200m, isSellRate: true)
        };

        var request = BuildRequest(
            new[] { operation },
            new Dictionary<string, CedearPriceRecord> { ["META"] = currentPrice },
            currentMep,
            currentCcl,
            historicalMep,
            historicalCcl);

        var result = new CalculationEngine().Calculate(request);
        var summary = Assert.Single(result.Tickers);

        Assert.Equal(120m, summary.CurrentValuation.CurrentValueUsdMep!.FxRate.RateValue);
        Assert.Equal(new DateOnly(2024, 1, 12), summary.CurrentValuation.CurrentValueUsdMep!.FxRate.RateDate);
        Assert.Equal(240m, summary.CurrentValuation.CurrentValueUsdCcl!.FxRate.RateValue);
        Assert.Equal(new DateOnly(2024, 1, 12), summary.CurrentValuation.CurrentValueUsdCcl!.FxRate.RateDate);
    }

    private static CalculationRequest BuildRequest(
        IReadOnlyList<Operation> operations,
        IReadOnlyDictionary<string, CedearPriceRecord> prices,
        FxRateRecord? currentMep,
        FxRateRecord? currentCcl,
        IReadOnlyDictionary<DateOnly, FxRateRecord> historicalMep,
        IReadOnlyDictionary<DateOnly, FxRateRecord> historicalCcl)
    {
        return new CalculationRequest(
            operations,
            prices,
            currentMep,
            currentCcl,
            historicalMep,
            historicalCcl);
    }

    private static Operation CreateOperation(DateOnly tradeDate, string ticker, decimal quantity, decimal priceArs, decimal? feesArs)
    {
        return new Operation(tradeDate, ticker, quantity, priceArs, feesArs);
    }

    private static CedearPriceRecord CreatePrice(string ticker, decimal priceArs)
    {
        return new CedearPriceRecord(ticker, priceArs, DateTimeOffset.UtcNow, "manual", isManual: true);
    }

    private static FxRateRecord CreateFx(FxRateType type, DateOnly date, decimal rate, bool isSellRate)
    {
        return new FxRateRecord(type, rate, date, DateTimeOffset.UtcNow, "manual", isManual: true, isSellRate);
    }
}
