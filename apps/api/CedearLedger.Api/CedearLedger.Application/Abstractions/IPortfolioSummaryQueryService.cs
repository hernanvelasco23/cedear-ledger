using CedearLedger.Domain.Models;

namespace CedearLedger.Application.Abstractions;

public interface IPortfolioSummaryQueryService
{
    Task<PortfolioSummary?> GetSummaryAsync(Guid portfolioId, CancellationToken cancellationToken);
}
