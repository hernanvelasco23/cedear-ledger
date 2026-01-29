using CedearLedger.Application.Abstractions;
using CedearLedger.Application.Portfolios;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class PortfolioRepository : IPortfolioRepository
{
    private readonly CedearLedgerDbContext _dbContext;

    public PortfolioRepository(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioDto> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var portfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Portfolios.Add(portfolio);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PortfolioDto(portfolio.Id, portfolio.Name, portfolio.CreatedAt);
    }
}
