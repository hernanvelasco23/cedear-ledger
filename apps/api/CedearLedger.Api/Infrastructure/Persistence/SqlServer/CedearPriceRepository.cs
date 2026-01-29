using CedearLedger.Application.Abstractions;
using CedearLedger.Application.CedearPrices;
using Microsoft.EntityFrameworkCore;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class CedearPriceRepository : ICedearPriceRepository
{
    private readonly CedearLedgerDbContext _dbContext;

    public CedearPriceRepository(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(string ticker, DateOnly priceDate, string source, CancellationToken cancellationToken)
    {
        return await _dbContext.CedearPrices
            .AsNoTracking()
            .AnyAsync(
                price => price.Ticker == ticker && price.PriceDate == priceDate && price.Source == source,
                cancellationToken);
    }

    public async Task<CedearPriceDto> UpsertAsync(UpsertCedearPriceData data, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.CedearPrices
            .FirstOrDefaultAsync(
                price => price.Ticker == data.Ticker && price.PriceDate == data.PriceDate,
                cancellationToken);

        if (existing is null)
        {
            existing = new CedearPrice
            {
                Id = Guid.NewGuid(),
                Ticker = data.Ticker,
                PriceArs = data.PriceArs,
                PriceDate = data.PriceDate,
                IsManual = data.IsManual,
                Source = data.Source,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.CedearPrices.Add(existing);
        }
        else
        {
            existing.PriceArs = data.PriceArs;
            existing.IsManual = data.IsManual;
            existing.Source = data.Source;
            existing.CreatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CedearPriceDto(
            existing.Id,
            existing.Ticker,
            existing.PriceArs,
            existing.PriceDate,
            existing.IsManual,
            existing.Source,
            existing.CreatedAt);
    }
}
