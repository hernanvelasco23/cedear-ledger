namespace CedearLedger.Domain.Models;

public sealed record PortfolioTotals(
    PortfolioInvestmentTotals InvestmentTotals,
    AggregateValuation CurrentValuation,
    ProfitAndLoss ProfitAndLoss,
    bool IsComplete
);
