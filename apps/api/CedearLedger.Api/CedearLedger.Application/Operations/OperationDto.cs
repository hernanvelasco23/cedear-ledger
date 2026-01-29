namespace CedearLedger.Application.Operations;

public sealed record OperationDto(
    Guid Id,
    Guid PortfolioId,
    string Ticker,
    decimal Quantity,
    decimal PriceArs,
    decimal FeesArs,
    DateOnly OperationDate,
    DateTime CreatedAt
);
