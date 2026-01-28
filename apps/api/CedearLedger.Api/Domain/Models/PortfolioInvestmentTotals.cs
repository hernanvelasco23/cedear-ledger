namespace CedearLedger.Domain.Models;

public sealed record PortfolioInvestmentTotals(
    decimal TotalInvestedArs,
    AggregateUsdValue? TotalInvestedUsdMep,
    AggregateUsdValue? TotalInvestedUsdCcl,
    bool IsComplete
);
