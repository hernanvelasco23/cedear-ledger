using CedearLedger.Application.Ingestion;

namespace CedearLedger.Application.Abstractions;

public interface IMarketDataProvider
{
    Task<ExternalDollarRatesResult> GetDollarRatesAsync(DateOnly date, CancellationToken cancellationToken);
    Task<ExternalCedearPricesResult> GetCedearPricesAsync(DateOnly date, IReadOnlyList<string> tickers, CancellationToken cancellationToken);
}
