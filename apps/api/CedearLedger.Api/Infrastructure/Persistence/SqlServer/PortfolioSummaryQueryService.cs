using CedearLedger.Application.Abstractions;
using CedearLedger.Domain.Models;
using CedearLedger.Domain.Services;
using Microsoft.EntityFrameworkCore;
using PersistenceOperation = CedearLedger.Infrastructure.Persistence.SqlServer.Operation;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class PortfolioSummaryQueryService : IPortfolioSummaryQueryService
{
    private readonly CedearLedgerDbContext _dbContext;
    private readonly CalculationEngine _engine = new();

    public PortfolioSummaryQueryService(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioSummary?> GetSummaryAsync(Guid portfolioId, CancellationToken cancellationToken)
    {
        var portfolioExists = await _dbContext.Portfolios
            .AsNoTracking()
            .AnyAsync(portfolio => portfolio.Id == portfolioId, cancellationToken);

        if (!portfolioExists)
        {
            return null;
        }

        var operations = await _dbContext.Operations
            .AsNoTracking()
            .Where(operation => operation.PortfolioId == portfolioId)
            .ToListAsync(cancellationToken);

        var operationModels = operations.Select(MapOperation).ToList();
        var tickerSet = operations
            .Select(operation => operation.Ticker)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var latestPrices = tickerSet.Count == 0
            ? new List<CedearPrice>()
            : await _dbContext.CedearPrices
                .AsNoTracking()
                .Where(price => tickerSet.Contains(price.Ticker))
                .GroupBy(price => price.Ticker)
                .Select(group => group
                    .OrderByDescending(price => price.PriceDate)
                    .ThenByDescending(price => price.CreatedAt)
                    .First())
                .ToListAsync(cancellationToken);

        var priceModels = latestPrices.ToDictionary(
            price => price.Ticker,
            price => new CedearPriceRecord(
                price.Ticker,
                price.PriceArs,
                new DateTimeOffset(price.CreatedAt, TimeSpan.Zero),
                price.Source,
                price.IsManual),
            StringComparer.OrdinalIgnoreCase);

        var dollarRates = await _dbContext.DollarRates
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var historicalMep = dollarRates
            .Where(rate => rate.DollarType == DollarType.Mep)
            .GroupBy(rate => rate.RateDate)
            .Select(group => group.OrderByDescending(rate => rate.CreatedAt).First())
            .ToDictionary(
                rate => rate.RateDate,
                rate => MapFxRate(rate, FxRateType.Mep));

        var historicalCcl = dollarRates
            .Where(rate => rate.DollarType == DollarType.Ccl)
            .GroupBy(rate => rate.RateDate)
            .Select(group => group.OrderByDescending(rate => rate.CreatedAt).First())
            .ToDictionary(
                rate => rate.RateDate,
                rate => MapFxRate(rate, FxRateType.Ccl));

        var currentMep = dollarRates
            .Where(rate => rate.DollarType == DollarType.Mep)
            .OrderByDescending(rate => rate.RateDate)
            .ThenByDescending(rate => rate.CreatedAt)
            .Select(rate => MapFxRate(rate, FxRateType.Mep))
            .FirstOrDefault();

        var currentCcl = dollarRates
            .Where(rate => rate.DollarType == DollarType.Ccl)
            .OrderByDescending(rate => rate.RateDate)
            .ThenByDescending(rate => rate.CreatedAt)
            .Select(rate => MapFxRate(rate, FxRateType.Ccl))
            .FirstOrDefault();

        var calculationRequest = new CalculationRequest(
            operationModels,
            priceModels,
            currentMep,
            currentCcl,
            historicalMep,
            historicalCcl);

        return _engine.Calculate(calculationRequest);
    }

    private static CedearLedger.Domain.Models.Operation MapOperation(PersistenceOperation operation)
    {
        return new CedearLedger.Domain.Models.Operation(
            operation.OperationDate,
            operation.Ticker,
            operation.Quantity,
            operation.PriceArs,
            operation.FeesArs);
    }

    private static FxRateRecord MapFxRate(DollarRate rate, FxRateType fxType)
    {
        return new FxRateRecord(
            fxType,
            rate.Rate,
            rate.RateDate,
            new DateTimeOffset(rate.CreatedAt, TimeSpan.Zero),
            rate.Source,
            rate.IsManual,
            IsSellRate: true);
    }
}
