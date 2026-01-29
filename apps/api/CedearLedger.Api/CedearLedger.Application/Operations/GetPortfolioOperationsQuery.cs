using MediatR;

namespace CedearLedger.Application.Operations;

public sealed record GetPortfolioOperationsQuery(Guid PortfolioId) : IRequest<IReadOnlyList<OperationDto>?>;
