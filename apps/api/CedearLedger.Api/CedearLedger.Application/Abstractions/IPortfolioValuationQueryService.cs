using CedearLedger.Application.Portfolios;

namespace CedearLedger.Application.Abstractions;

public interface IPortfolioValuationQueryService
{
    Task<PortfolioValuationDto> GetValuationAsync(Guid portfolioId, CancellationToken ct);
}
