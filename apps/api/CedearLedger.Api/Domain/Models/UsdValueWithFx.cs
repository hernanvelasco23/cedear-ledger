namespace CedearLedger.Domain.Models;

public sealed record UsdValueWithFx(
    decimal Value,
    FxRateRecord FxRate
);
