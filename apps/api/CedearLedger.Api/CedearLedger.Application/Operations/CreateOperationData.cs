namespace CedearLedger.Application.Operations;

public sealed record CreateOperationData(
    Guid PortfolioId,
    string Ticker,
    decimal Quantity,
    decimal PriceArs,
    decimal FeesArs,
    DateOnly OperationDate
);
