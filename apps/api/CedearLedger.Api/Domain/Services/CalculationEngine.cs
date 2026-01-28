using CedearLedger.Domain.Models;

namespace CedearLedger.Domain.Services;

public sealed class CalculationEngine
{
    public PortfolioSummary Calculate(CalculationRequest request)
    {
        var tickerSummaries = request.Operations
            .GroupBy(operation => operation.Ticker, StringComparer.OrdinalIgnoreCase)
            .Select(group => BuildTickerSummary(
                group.Key,
                group.ToList(),
                request.CurrentPricesByTicker,
                request.CurrentMep,
                request.CurrentCcl,
                request.HistoricalMepByDate,
                request.HistoricalCclByDate))
            .OrderBy(summary => summary.Ticker, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var portfolioTotals = BuildPortfolioTotals(tickerSummaries, request.CurrentMep, request.CurrentCcl);

        return new PortfolioSummary(tickerSummaries, portfolioTotals);
    }

    private static TickerSummary BuildTickerSummary(
        string ticker,
        IReadOnlyList<Operation> operations,
        IReadOnlyDictionary<string, CedearPriceRecord> currentPricesByTicker,
        FxRateRecord? currentMep,
        FxRateRecord? currentCcl,
        IReadOnlyDictionary<DateOnly, FxRateRecord> historicalMepByDate,
        IReadOnlyDictionary<DateOnly, FxRateRecord> historicalCclByDate)
    {
        var operationCosts = operations
            .Select(operation => BuildOperationCost(operation, historicalMepByDate, historicalCclByDate))
            .ToList();

        var totalQuantity = operations.Sum(operation => operation.Quantity);
        var totalInvestedArs = operationCosts.Sum(cost => cost.ArsCost);
        var avgPriceArs = totalQuantity == 0m
            ? 0m
            : operations.Sum(operation => operation.Quantity * operation.PriceArs) / totalQuantity;

        var totalInvestedUsdMep = BuildAggregateUsdValue(
            FxRateType.Mep,
            totalInvestedArs,
            operationCosts.Select(cost => cost.UsdCostMep).ToList());

        var totalInvestedUsdCcl = BuildAggregateUsdValue(
            FxRateType.Ccl,
            totalInvestedArs,
            operationCosts.Select(cost => cost.UsdCostCcl).ToList());

        var investmentTotalsComplete = IsAggregateComplete(operationCosts.Count, totalInvestedUsdMep, totalInvestedUsdCcl);
        var investmentTotals = new InvestmentTotals(
            totalInvestedArs,
            totalInvestedUsdMep,
            totalInvestedUsdCcl,
            avgPriceArs,
            investmentTotalsComplete);

        currentPricesByTicker.TryGetValue(ticker, out var currentPrice);
        decimal? currentValueArs = currentPrice is null ? null : totalQuantity * currentPrice.PriceArs;

        var currentValueUsdMep = BuildCurrentUsdValue(currentValueArs, currentMep);
        var currentValueUsdCcl = BuildCurrentUsdValue(currentValueArs, currentCcl);
        var currentValuationComplete = currentValueArs is not null && currentValueUsdMep is not null && currentValueUsdCcl is not null;

        var currentValuation = new CurrentValuation(
            currentPrice,
            currentValueArs,
            currentValueUsdMep,
            currentValueUsdCcl,
            currentValuationComplete);

        decimal? pnlArs = currentValueArs is null ? null : currentValueArs.Value - totalInvestedArs;
        var pnlUsdMep = BuildUsdPnl(currentValueUsdMep, totalInvestedUsdMep);
        var pnlUsdCcl = BuildUsdPnl(currentValueUsdCcl, totalInvestedUsdCcl);
        var pnlComplete = pnlArs is not null && pnlUsdMep is not null && pnlUsdCcl is not null;

        var profitAndLoss = new ProfitAndLoss(
            pnlArs,
            pnlUsdMep,
            pnlUsdCcl,
            pnlComplete);

        return new TickerSummary(
            ticker,
            totalQuantity,
            investmentTotals,
            currentValuation,
            profitAndLoss,
            operationCosts);
    }

    private static OperationCost BuildOperationCost(
        Operation operation,
        IReadOnlyDictionary<DateOnly, FxRateRecord> historicalMepByDate,
        IReadOnlyDictionary<DateOnly, FxRateRecord> historicalCclByDate)
    {
        var fees = operation.FeesArs ?? 0m;
        var arsCost = (operation.Quantity * operation.PriceArs) + fees;

        var mepRate = FindHistoricalRate(operation.TradeDate, historicalMepByDate);
        var cclRate = FindHistoricalRate(operation.TradeDate, historicalCclByDate);

        var usdCostMep = mepRate is null ? null : new UsdValueWithFx(arsCost / mepRate.RateValue, mepRate);
        var usdCostCcl = cclRate is null ? null : new UsdValueWithFx(arsCost / cclRate.RateValue, cclRate);
        var isComplete = usdCostMep is not null && usdCostCcl is not null;

        return new OperationCost(operation, arsCost, usdCostMep, usdCostCcl, isComplete);
    }

    private static FxRateRecord? FindHistoricalRate(DateOnly tradeDate, IReadOnlyDictionary<DateOnly, FxRateRecord> rates)
    {
        if (rates.Count == 0)
        {
            return null;
        }

        if (rates.TryGetValue(tradeDate, out var exact) && exact.IsSellRate)
        {
            return exact;
        }

        var probeDate = tradeDate.AddDays(-1);
        while (true)
        {
            if (rates.TryGetValue(probeDate, out var candidate))
            {
                return candidate.IsSellRate ? candidate : null;
            }

            if (probeDate <= DateOnly.MinValue.AddDays(1))
            {
                return null;
            }

            probeDate = probeDate.AddDays(-1);
        }
    }

    private static AggregateUsdValue? BuildAggregateUsdValue(
        FxRateType fxType,
        decimal totalInvestedArs,
        IReadOnlyList<UsdValueWithFx?> usdCosts)
    {
        if (usdCosts.Count == 0)
        {
            return new AggregateUsdValue(fxType, 0m, 0m, Array.Empty<FxRateRecord>());
        }

        if (usdCosts.Any(cost => cost is null))
        {
            return null;
        }

        var realizedCosts = usdCosts.Select(cost => cost!).ToList();
        var totalUsd = realizedCosts.Sum(cost => cost.Value);
        var weightedRate = totalUsd == 0m ? 0m : totalInvestedArs / totalUsd;
        var ratesUsed = realizedCosts.Select(cost => cost.FxRate).ToList();

        return new AggregateUsdValue(fxType, totalUsd, weightedRate, ratesUsed);
    }

    private static UsdValueWithFx? BuildCurrentUsdValue(decimal? currentValueArs, FxRateRecord? currentFx)
    {
        if (currentValueArs is null || currentFx is null || !currentFx.IsSellRate)
        {
            return null;
        }

        return new UsdValueWithFx(currentValueArs.Value / currentFx.RateValue, currentFx);
    }


    private static UsdPnl? BuildUsdPnl(UsdValueWithFx? currentValue, AggregateUsdValue? totalInvested)
    {
        if (currentValue is null || totalInvested is null)
        {
            return null;
        }

        return new UsdPnl(currentValue.Value - totalInvested.Value, currentValue, totalInvested);
    }

    private static bool IsAggregateComplete(int operationCount, AggregateUsdValue? mep, AggregateUsdValue? ccl)
    {
        if (operationCount == 0)
        {
            return true;
        }

        return mep is not null && ccl is not null;
    }

    private static PortfolioTotals BuildPortfolioTotals(
        IReadOnlyList<TickerSummary> tickerSummaries,
        FxRateRecord? currentMep,
        FxRateRecord? currentCcl)
    {
        var totalInvestedArs = tickerSummaries.Sum(summary => summary.Totals.TotalInvestedArs);
        var totalInvestedUsdMep = BuildAggregateFromTotals(
            FxRateType.Mep,
            totalInvestedArs,
            tickerSummaries.Select(summary => summary.Totals.TotalInvestedUsdMep).ToList());

        var totalInvestedUsdCcl = BuildAggregateFromTotals(
            FxRateType.Ccl,
            totalInvestedArs,
            tickerSummaries.Select(summary => summary.Totals.TotalInvestedUsdCcl).ToList());

        var investmentTotalsComplete = tickerSummaries.All(summary => summary.Totals.IsComplete);
        var investmentTotals = new PortfolioInvestmentTotals(
            totalInvestedArs,
            totalInvestedUsdMep,
            totalInvestedUsdCcl,
            investmentTotalsComplete);

        var currentValuesArs = tickerSummaries.Select(summary => summary.CurrentValuation.CurrentValueArs).ToList();
        var allCurrentArsAvailable = currentValuesArs.All(value => value is not null);
        decimal? currentValueArs = allCurrentArsAvailable ? currentValuesArs.Sum(value => value ?? 0m) : null;

        var currentValueUsdMep = BuildCurrentUsdValue(currentValueArs, currentMep);
        var currentValueUsdCcl = BuildCurrentUsdValue(currentValueArs, currentCcl);
        var currentValuationComplete = currentValueArs is not null && currentValueUsdMep is not null && currentValueUsdCcl is not null;

        var currentValuation = new AggregateValuation(
            currentValueArs,
            currentValueUsdMep,
            currentValueUsdCcl,
            currentValuationComplete);

        decimal? pnlArs = currentValueArs is null ? null : currentValueArs.Value - totalInvestedArs;
        var pnlUsdMep = BuildUsdPnl(currentValueUsdMep, totalInvestedUsdMep);
        var pnlUsdCcl = BuildUsdPnl(currentValueUsdCcl, totalInvestedUsdCcl);
        var pnlComplete = pnlArs is not null && pnlUsdMep is not null && pnlUsdCcl is not null;

        var profitAndLoss = new ProfitAndLoss(
            pnlArs,
            pnlUsdMep,
            pnlUsdCcl,
            pnlComplete);

        var isComplete = investmentTotalsComplete && currentValuationComplete && pnlComplete;
        return new PortfolioTotals(investmentTotals, currentValuation, profitAndLoss, isComplete);
    }

    private static AggregateUsdValue? BuildAggregateFromTotals(
        FxRateType fxType,
        decimal totalInvestedArs,
        IReadOnlyList<AggregateUsdValue?> totals)
    {
        if (totals.Count == 0)
        {
            return new AggregateUsdValue(fxType, 0m, 0m, Array.Empty<FxRateRecord>());
        }

        if (totals.Any(total => total is null))
        {
            return null;
        }

        var realizedTotals = totals.Select(total => total!).ToList();
        var totalUsd = realizedTotals.Sum(total => total.Value);
        var weightedRate = totalUsd == 0m ? 0m : totalInvestedArs / totalUsd;
        var ratesUsed = realizedTotals.SelectMany(total => total.FxRatesUsed).ToList();

        return new AggregateUsdValue(fxType, totalUsd, weightedRate, ratesUsed);
    }
}
