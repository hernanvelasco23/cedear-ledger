using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed record GetPortfolioValuationQuery(Guid PortfolioId) : IRequest<PortfolioValuationDto>;
