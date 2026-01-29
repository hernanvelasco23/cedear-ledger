using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed record CreatePortfolioCommand(string Name) : IRequest<PortfolioDto>;
