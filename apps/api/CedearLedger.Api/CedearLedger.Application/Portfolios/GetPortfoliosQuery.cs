using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed record GetPortfoliosQuery : IRequest<IReadOnlyList<PortfolioDto>>;
