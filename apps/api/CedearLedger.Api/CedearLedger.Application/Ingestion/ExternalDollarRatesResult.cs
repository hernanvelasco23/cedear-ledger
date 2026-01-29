namespace CedearLedger.Application.Ingestion;

public sealed record ExternalDollarRatesResult(
    DateOnly Date,
    ExternalDollarRate Mep,
    ExternalDollarRate Ccl
);

public sealed record ExternalDollarRate(
    decimal Rate,
    string Source
);
