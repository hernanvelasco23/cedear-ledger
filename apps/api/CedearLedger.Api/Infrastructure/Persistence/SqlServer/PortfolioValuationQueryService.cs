using CedearLedger.Application.Abstractions;
using CedearLedger.Application.Portfolios;
using Microsoft.EntityFrameworkCore;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class PortfolioValuationQueryService : IPortfolioValuationQueryService
{
    private readonly CedearLedgerDbContext _dbContext;

    public PortfolioValuationQueryService(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioValuationDto> GetValuationAsync(Guid portfolioId, CancellationToken ct)
    {
        var aggregates = await _dbContext.Operations
            .AsNoTracking()
            .Where(operation => operation.PortfolioId == portfolioId)
            .GroupBy(operation => operation.Ticker)
            .Select(group => new OperationAggregate(
                group.Key,
                group.Sum(op => op.Quantity > 0 ? op.Quantity : 0m),
                group.Sum(op => op.Quantity < 0 ? -op.Quantity : 0m),
                group.Sum(op => op.Quantity > 0 ? op.Quantity * op.PriceArs : 0m),
                group.Sum(op => op.Quantity < 0 ? -op.Quantity * op.PriceArs : 0m)))
            .ToListAsync(ct);

        if (aggregates.Count == 0)
        {
            var portfolioExists = await _dbContext.Portfolios
                .AsNoTracking()
                .AnyAsync(portfolio => portfolio.Id == portfolioId, ct);

            if (!portfolioExists)
            {
                throw new InvalidOperationException("Portfolio not found");
            }

            return ComputeValuation(
                portfolioId,
                aggregates,
                new Dictionary<string, PriceSnapshot>(StringComparer.OrdinalIgnoreCase),
                DateTime.UtcNow);
        }

        var tickers = aggregates
            .Select(aggregate => aggregate.Ticker)
            .ToList();

        var latestPrices = tickers.Count == 0
            ? new List<CedearPrice>()
            : await _dbContext.CedearPrices
                .AsNoTracking()
                .Where(price => tickers.Contains(price.Ticker))
                .GroupBy(price => price.Ticker)
                .Select(group => group
                    .OrderByDescending(price => price.PriceDate)
                    .ThenByDescending(price => price.CreatedAt)
                    .First())
                .ToListAsync(ct);

        var priceLookup = latestPrices.ToDictionary(
            price => price.Ticker,
            price => new PriceSnapshot(price.PriceArs, price.PriceDate),
            StringComparer.OrdinalIgnoreCase);

        return ComputeValuation(portfolioId, aggregates, priceLookup, DateTime.UtcNow);
    }

    internal static PortfolioValuationDto ComputeValuation(
        Guid portfolioId,
        IReadOnlyList<OperationAggregate> aggregates,
        IReadOnlyDictionary<string, PriceSnapshot> priceLookup,
        DateTime calculatedAt)
    {
        var positions = new List<TickerValuationDto>();
        decimal totalCost = 0m;
        decimal totalMarketValue = 0m;
        decimal totalUnrealized = 0m;

        foreach (var aggregate in aggregates)
        {
            var quantityNet = aggregate.BuyQuantity - aggregate.SellQuantity;
            if (quantityNet <= 0)
            {
                continue;
            }

            var costArs = aggregate.BuyCost - aggregate.SellCost;
            var avgCost = quantityNet > 0 && costArs > 0 ? costArs / quantityNet : 0m;

            if (priceLookup.TryGetValue(aggregate.Ticker, out var price))
            {
                var marketPrice = price.PriceArs;
                var marketValue = quantityNet * marketPrice;
                var unrealized = (marketPrice - avgCost) * quantityNet;
                var pnlPercent = avgCost > 0 ? marketPrice / avgCost - 1m : 0m;

                positions.Add(new TickerValuationDto(
                    aggregate.Ticker,
                    quantityNet,
                    avgCost,
                    costArs,
                    marketPrice,
                    marketValue,
                    unrealized,
                    pnlPercent,
                    price.PriceDate));

                totalCost += costArs;
                totalMarketValue += marketValue;
                totalUnrealized += unrealized;
            }
            else
            {
                positions.Add(new TickerValuationDto(
                    aggregate.Ticker,
                    quantityNet,
                    avgCost,
                    costArs,
                    0m,
                    0m,
                    0m,
                    0m,
                    null));

                totalCost += costArs;
            }
        }

        var totalUnrealizedPercent = totalCost > 0 ? totalMarketValue / totalCost - 1m : 0m;

        return new PortfolioValuationDto(
            portfolioId,
            calculatedAt,
            totalCost,
            totalMarketValue,
            totalUnrealized,
            totalUnrealizedPercent,
            positions);
    }

    internal sealed record OperationAggregate(
        string Ticker,
        decimal BuyQuantity,
        decimal SellQuantity,
        decimal BuyCost,
        decimal SellCost);

    internal sealed record PriceSnapshot(decimal PriceArs, DateOnly PriceDate);
}
