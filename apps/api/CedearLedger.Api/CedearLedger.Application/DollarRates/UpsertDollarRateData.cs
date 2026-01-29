using CedearLedger.Domain.Models;

namespace CedearLedger.Application.DollarRates;

public sealed record UpsertDollarRateData(
    DollarType DollarType,
    decimal Rate,
    DateOnly RateDate,
    bool IsManual,
    string Source
);
