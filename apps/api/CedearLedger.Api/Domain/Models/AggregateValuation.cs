namespace CedearLedger.Domain.Models;

public sealed record AggregateValuation(
    decimal? CurrentValueArs,
    UsdValueWithFx? CurrentValueUsdMep,
    UsdValueWithFx? CurrentValueUsdCcl,
    bool IsComplete
);
