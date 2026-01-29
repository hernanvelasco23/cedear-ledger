using CedearLedger.Application.Abstractions;
using CedearLedger.Application.DollarRates;
using CedearLedger.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class DollarRateRepository : IDollarRateRepository
{
    private readonly CedearLedgerDbContext _dbContext;

    public DollarRateRepository(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsAsync(DollarType dollarType, DateOnly rateDate, string source, CancellationToken cancellationToken)
    {
        return await _dbContext.DollarRates
            .AsNoTracking()
            .AnyAsync(
                rate => rate.DollarType == dollarType && rate.RateDate == rateDate && rate.Source == source,
                cancellationToken);
    }

    public async Task<DollarRateDto> UpsertAsync(UpsertDollarRateData data, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.DollarRates
            .FirstOrDefaultAsync(
                rate => rate.DollarType == data.DollarType && rate.RateDate == data.RateDate,
                cancellationToken);

        if (existing is null)
        {
            existing = new DollarRate
            {
                Id = Guid.NewGuid(),
                DollarType = data.DollarType,
                Rate = data.Rate,
                RateDate = data.RateDate,
                IsManual = data.IsManual,
                Source = data.Source,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.DollarRates.Add(existing);
        }
        else
        {
            existing.Rate = data.Rate;
            existing.IsManual = data.IsManual;
            existing.Source = data.Source;
            existing.CreatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DollarRateDto(
            existing.Id,
            existing.DollarType,
            existing.Rate,
            existing.RateDate,
            existing.IsManual,
            existing.Source,
            existing.CreatedAt);
    }
}
