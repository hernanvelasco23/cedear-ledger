namespace CedearLedger.Domain.Models;

public sealed record CalculationRequest(
    IReadOnlyList<Operation> Operations,
    IReadOnlyDictionary<string, CedearPriceRecord> CurrentPricesByTicker,
    FxRateRecord? CurrentMep,
    FxRateRecord? CurrentCcl,
    IReadOnlyDictionary<DateOnly, FxRateRecord> HistoricalMepByDate,
    IReadOnlyDictionary<DateOnly, FxRateRecord> HistoricalCclByDate
);
