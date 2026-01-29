using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed record GetPortfolioByIdQuery(Guid PortfolioId) : IRequest<PortfolioDto?>;
