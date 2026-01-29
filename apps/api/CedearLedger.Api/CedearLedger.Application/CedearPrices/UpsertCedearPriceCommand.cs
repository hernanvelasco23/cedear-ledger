using MediatR;

namespace CedearLedger.Application.CedearPrices;

public sealed record UpsertCedearPriceCommand(
    string Ticker,
    decimal PriceArs,
    DateOnly PriceDate,
    bool IsManual,
    string Source
) : IRequest<CedearPriceDto>;
