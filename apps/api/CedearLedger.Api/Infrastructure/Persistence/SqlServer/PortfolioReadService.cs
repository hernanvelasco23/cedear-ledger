using CedearLedger.Application.Abstractions;
using CedearLedger.Application.Operations;
using CedearLedger.Application.Portfolios;
using Microsoft.EntityFrameworkCore;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class PortfolioReadService : IPortfolioReadService
{
    private readonly CedearLedgerDbContext _dbContext;

    public PortfolioReadService(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PortfolioDto>> GetPortfoliosAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Portfolios
            .AsNoTracking()
            .OrderByDescending(portfolio => portfolio.CreatedAt)
            .Select(portfolio => new PortfolioDto(
                portfolio.Id,
                portfolio.Name,
                portfolio.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PortfolioDto?> GetPortfolioByIdAsync(Guid portfolioId, CancellationToken cancellationToken)
    {
        return await _dbContext.Portfolios
            .AsNoTracking()
            .Where(portfolio => portfolio.Id == portfolioId)
            .Select(portfolio => new PortfolioDto(
                portfolio.Id,
                portfolio.Name,
                portfolio.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OperationDto>?> GetPortfolioOperationsAsync(Guid portfolioId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Portfolios
            .AsNoTracking()
            .AnyAsync(portfolio => portfolio.Id == portfolioId, cancellationToken);

        if (!exists)
        {
            return null;
        }

        return await _dbContext.Operations
            .AsNoTracking()
            .Where(operation => operation.PortfolioId == portfolioId)
            .OrderByDescending(operation => operation.OperationDate)
            .ThenByDescending(operation => operation.CreatedAt)
            .Select(operation => new OperationDto(
                operation.Id,
                operation.PortfolioId,
                operation.Ticker,
                operation.Quantity,
                operation.PriceArs,
                operation.FeesArs,
                operation.OperationDate,
                operation.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
