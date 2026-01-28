namespace CedearLedger.Domain.Models;

public sealed record PortfolioSummary(
    IReadOnlyList<TickerSummary> Tickers,
    PortfolioTotals Totals
);
