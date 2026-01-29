using CedearLedger.Application.Abstractions;
using CedearLedger.Application.Operations;
using Microsoft.EntityFrameworkCore;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class OperationRepository : IOperationRepository
{
    private readonly CedearLedgerDbContext _dbContext;

    public OperationRepository(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OperationDto> CreateAsync(CreateOperationData data, CancellationToken cancellationToken)
    {
        var portfolioExists = await _dbContext.Portfolios
            .AnyAsync(portfolio => portfolio.Id == data.PortfolioId, cancellationToken);

        if (!portfolioExists)
        {
            throw new KeyNotFoundException("Portfolio not found.");
        }

        var operation = new Operation
        {
            Id = Guid.NewGuid(),
            PortfolioId = data.PortfolioId,
            Ticker = data.Ticker,
            Quantity = data.Quantity,
            PriceArs = data.PriceArs,
            FeesArs = data.FeesArs,
            OperationDate = data.OperationDate,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Operations.Add(operation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new OperationDto(
            operation.Id,
            operation.PortfolioId,
            operation.Ticker,
            operation.Quantity,
            operation.PriceArs,
            operation.FeesArs,
            operation.OperationDate,
            operation.CreatedAt);
    }
}
