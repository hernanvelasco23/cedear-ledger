namespace CedearLedger.Domain.Models;

public sealed record FxRateRecord(
    FxRateType RateType,
    decimal RateValue,
    DateOnly RateDate,
    DateTimeOffset RetrievedAt,
    string Source,
    bool IsManual,
    bool IsSellRate
);
