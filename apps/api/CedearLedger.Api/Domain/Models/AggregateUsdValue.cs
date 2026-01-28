namespace CedearLedger.Domain.Models;

public sealed record AggregateUsdValue(
    FxRateType FxType,
    decimal Value,
    decimal WeightedRate,
    IReadOnlyList<FxRateRecord> FxRatesUsed
);
