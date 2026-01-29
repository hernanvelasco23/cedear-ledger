namespace CedearLedger.Application.CedearPrices;

public sealed record CedearPriceDto(
    Guid Id,
    string Ticker,
    decimal PriceArs,
    DateOnly PriceDate,
    bool IsManual,
    string Source,
    DateTime CreatedAt
);
