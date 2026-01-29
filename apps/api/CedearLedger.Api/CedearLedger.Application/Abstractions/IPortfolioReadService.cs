using CedearLedger.Application.Operations;
using CedearLedger.Application.Portfolios;

namespace CedearLedger.Application.Abstractions;

public interface IPortfolioReadService
{
    Task<IReadOnlyList<PortfolioDto>> GetPortfoliosAsync(CancellationToken cancellationToken);
    Task<PortfolioDto?> GetPortfolioByIdAsync(Guid portfolioId, CancellationToken cancellationToken);
    Task<IReadOnlyList<OperationDto>?> GetPortfolioOperationsAsync(Guid portfolioId, CancellationToken cancellationToken);
}
