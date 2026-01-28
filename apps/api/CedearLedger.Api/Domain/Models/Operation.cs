namespace CedearLedger.Domain.Models;

public sealed record Operation(
    DateOnly TradeDate,
    string Ticker,
    decimal Quantity,
    decimal PriceArs,
    decimal? FeesArs
);
