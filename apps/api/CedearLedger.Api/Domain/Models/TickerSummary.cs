namespace CedearLedger.Domain.Models;

public sealed record TickerSummary(
    string Ticker,
    decimal TotalQuantity,
    InvestmentTotals Totals,
    CurrentValuation CurrentValuation,
    ProfitAndLoss ProfitAndLoss,
    IReadOnlyList<OperationCost> OperationCosts
);
