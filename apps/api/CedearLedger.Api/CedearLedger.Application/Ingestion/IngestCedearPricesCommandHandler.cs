using CedearLedger.Application.Abstractions;
using CedearLedger.Application.CedearPrices;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CedearLedger.Application.Ingestion;

public sealed class IngestCedearPricesCommandHandler : IRequestHandler<IngestCedearPricesCommand, IngestionResult>
{
    private readonly IMarketDataProvider _provider;
    private readonly IMediator _mediator;
    private readonly ICedearPriceRepository _cedearPriceRepository;
    private readonly ILogger<IngestCedearPricesCommandHandler> _logger;

    public IngestCedearPricesCommandHandler(
        IMarketDataProvider provider,
        IMediator mediator,
        ICedearPriceRepository cedearPriceRepository,
        ILogger<IngestCedearPricesCommandHandler> logger)
    {
        _provider = provider;
        _mediator = mediator;
        _cedearPriceRepository = cedearPriceRepository;
        _logger = logger;
    }

    public async Task<IngestionResult> Handle(IngestCedearPricesCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var errors = new List<string>();
        var details = new List<IngestionDetail>();
        var inserted = 0;
        var skipped = 0;
        var failed = 0;
        var attempted = 0;

        ExternalCedearPricesResult data;
        try
        {
            data = await _provider.GetCedearPricesAsync(request.Date, request.Tickers, cancellationToken);
        }
        catch (Exception ex)
        {
            return new IngestionResult(request.Date, 0, 0, new[] { ex.Message }, Array.Empty<IngestionDetail>());
        }

        foreach (var price in data.Prices)
        {
            var key = $"{price.Ticker}:{data.Date:yyyy-MM-dd}";
            attempted++;

            if (string.IsNullOrWhiteSpace(price.Ticker) || price.PriceArs <= 0 || string.IsNullOrWhiteSpace(price.Source))
            {
                skipped++;
                details.Add(new IngestionDetail(key, "skipped", "Invalid ticker, price, or source"));
                continue;
            }

            try
            {
                if (price.PriceArs <= 0m ||
                    price.PriceArs >= 5_000_000m ||
                    string.IsNullOrWhiteSpace(price.Currency))
                {
                    skipped++;
                    _logger.LogWarning(
                        "Skipped invalid cedear price {Ticker} {Date}. Price={Price} Currency={Currency}",
                        price.Ticker,
                        data.Date,
                        price.PriceArs,
                        price.Currency);
                    details.Add(new IngestionDetail(key, "skipped", "Invalid price"));
                    continue;
                }

                var exists = await _cedearPriceRepository.ExistsAsync(
                    price.Ticker,
                    data.Date,
                    price.Source,
                    cancellationToken);

                if (exists)
                {
                    skipped++;
                    _logger.LogInformation("Skipped duplicate cedear price {Ticker} {PriceDate}", price.Ticker, data.Date);
                    details.Add(new IngestionDetail(key, "skipped", "Duplicate price"));
                    continue;
                }

                var command = new UpsertCedearPriceCommand(
                    price.Ticker,
                    price.PriceArs,
                    data.Date,
                    request.ForceManualFlag,
                    price.Source);
                await _mediator.Send(command, cancellationToken);
                inserted++;
                details.Add(new IngestionDetail(key, "upserted", null));
            }
            catch (Exception ex)
            {
                failed++;
                errors.Add($"{key}: {ex.Message}");
                details.Add(new IngestionDetail(key, "error", ex.Message));
            }
        }

        stopwatch.Stop();
        _logger.LogInformation(
            "CedearPrices ingestion summary for {Date}. Attempted={Attempted}, Inserted={Inserted}, Skipped={Skipped}, Failed={Failed}, DurationMs={DurationMs}",
            data.Date,
            attempted,
            inserted,
            skipped,
            failed,
            stopwatch.ElapsedMilliseconds);

        return new IngestionResult(data.Date, inserted, skipped, errors, details);
    }
}
