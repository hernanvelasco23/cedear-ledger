using CedearLedger.Application.CedearPrices;

namespace CedearLedger.Application.Abstractions;

public interface ICedearPriceRepository
{
    Task<bool> ExistsAsync(string ticker, DateOnly priceDate, string source, CancellationToken cancellationToken);
    Task<CedearPriceDto> UpsertAsync(UpsertCedearPriceData data, CancellationToken cancellationToken);
}
