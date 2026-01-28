namespace CedearLedger.Domain.Models;

public sealed record UsdPnl(
    decimal Value,
    UsdValueWithFx CurrentValue,
    AggregateUsdValue TotalInvested
);
