using CedearLedger.Application.DollarRates;
using CedearLedger.Domain.Models;

namespace CedearLedger.Application.Abstractions;

public interface IDollarRateRepository
{
    Task<bool> ExistsAsync(DollarType dollarType, DateOnly rateDate, string source, CancellationToken cancellationToken);
    Task<DollarRateDto> UpsertAsync(UpsertDollarRateData data, CancellationToken cancellationToken);
}
