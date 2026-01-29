namespace CedearLedger.Application.CedearPrices;

public sealed record UpsertCedearPriceData(
    string Ticker,
    decimal PriceArs,
    DateOnly PriceDate,
    bool IsManual,
    string Source
);
