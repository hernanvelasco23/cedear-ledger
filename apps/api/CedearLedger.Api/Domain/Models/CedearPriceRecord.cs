namespace CedearLedger.Domain.Models;

public sealed record CedearPriceRecord(
    string Ticker,
    decimal PriceArs,
    DateTimeOffset AsOf,
    string Source,
    bool IsManual
);
