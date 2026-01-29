using CedearLedger.Domain.Models;
using MediatR;

namespace CedearLedger.Application.DollarRates;

public sealed record UpsertDollarRateCommand(
    DollarType DollarType,
    decimal Rate,
    DateOnly RateDate,
    bool IsManual,
    string Source
) : IRequest<DollarRateDto>;
