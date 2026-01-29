namespace CedearLedger.Application.Ingestion;

public sealed record ExternalCedearPricesResult(
    DateOnly Date,
    IReadOnlyList<ExternalCedearPrice> Prices
);

public sealed record ExternalCedearPrice(
    string Ticker,
    decimal PriceArs,
    string Source,
    string Currency
);
