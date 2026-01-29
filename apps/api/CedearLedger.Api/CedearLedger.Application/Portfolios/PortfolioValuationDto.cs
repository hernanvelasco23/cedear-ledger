namespace CedearLedger.Application.Portfolios;

public sealed record PortfolioValuationDto(
    Guid PortfolioId,
    DateTime CalculatedAt,
    decimal TotalCostArs,
    decimal TotalMarketValueArs,
    decimal TotalUnrealizedPnLArs,
    decimal TotalUnrealizedPnLPercent,
    IReadOnlyList<TickerValuationDto> Positions);

public sealed record TickerValuationDto(
    string Ticker,
    decimal Quantity,
    decimal AvgCostArs,
    decimal CostArs,
    decimal MarketPriceArs,
    decimal MarketValueArs,
    decimal UnrealizedPnLArs,
    decimal UnrealizedPnLPercent,
    DateOnly? MarketPriceDate);
