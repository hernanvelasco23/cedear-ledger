using CedearLedger.Domain.Models;

namespace CedearLedger.Application.DollarRates;

public sealed record DollarRateDto(
    Guid Id,
    DollarType DollarType,
    decimal Rate,
    DateOnly RateDate,
    bool IsManual,
    string Source,
    DateTime CreatedAt
);
