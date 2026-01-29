using MediatR;

namespace CedearLedger.Application.Operations;

public sealed record CreateOperationCommand(
    Guid PortfolioId,
    string Ticker,
    decimal Quantity,
    decimal PriceArs,
    decimal FeesArs,
    DateOnly OperationDate
) : IRequest<OperationDto?>;
