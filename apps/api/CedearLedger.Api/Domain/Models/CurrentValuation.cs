namespace CedearLedger.Domain.Models;

public sealed record CurrentValuation(
    CedearPriceRecord? Price,
    decimal? CurrentValueArs,
    UsdValueWithFx? CurrentValueUsdMep,
    UsdValueWithFx? CurrentValueUsdCcl,
    bool IsComplete
);
