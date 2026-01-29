using CedearLedger.Application.Portfolios;

namespace CedearLedger.Application.Abstractions;

public interface IPortfolioRepository
{
    Task<PortfolioDto> CreateAsync(string name, CancellationToken cancellationToken);
}
