namespace CedearLedger.Domain.Models;

public sealed record InvestmentTotals(
    decimal TotalInvestedArs,
    AggregateUsdValue? TotalInvestedUsdMep,
    AggregateUsdValue? TotalInvestedUsdCcl,
    decimal AvgPriceArs,
    bool IsComplete
);
