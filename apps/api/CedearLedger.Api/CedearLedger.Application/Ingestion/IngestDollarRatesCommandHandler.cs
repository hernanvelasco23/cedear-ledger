using CedearLedger.Application.Abstractions;
using CedearLedger.Application.DollarRates;
using CedearLedger.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace CedearLedger.Application.Ingestion;

public sealed class IngestDollarRatesCommandHandler : IRequestHandler<IngestDollarRatesCommand, IngestionResult>
{
    private readonly IMarketDataProvider _provider;
    private readonly IMediator _mediator;
    private readonly IDollarRateRepository _dollarRateRepository;
    private readonly ILogger<IngestDollarRatesCommandHandler> _logger;

    public IngestDollarRatesCommandHandler(
        IMarketDataProvider provider,
        IMediator mediator,
        IDollarRateRepository dollarRateRepository,
        ILogger<IngestDollarRatesCommandHandler> logger)
    {
        _provider = provider;
        _mediator = mediator;
        _dollarRateRepository = dollarRateRepository;
        _logger = logger;
    }

    public async Task<IngestionResult> Handle(IngestDollarRatesCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var errors = new List<string>();
        var details = new List<IngestionDetail>();
        var inserted = 0;
        var skipped = 0;
        var failed = 0;
        var attempted = 2;

        ExternalDollarRatesResult data;
        try
        {
            data = await _provider.GetDollarRatesAsync(request.Date, cancellationToken);
        }
        catch (Exception ex)
        {
            return new IngestionResult(request.Date, 0, 0, new[] { ex.Message }, Array.Empty<IngestionDetail>());
        }

        var mepResult = await ProcessRateAsync(DollarType.Mep, data.Mep, data.Date, request.ForceManualFlag, errors, details, cancellationToken);
        inserted += mepResult.inserted;
        skipped += mepResult.skipped;
        failed += mepResult.failed;

        var cclResult = await ProcessRateAsync(DollarType.Ccl, data.Ccl, data.Date, request.ForceManualFlag, errors, details, cancellationToken);
        inserted += cclResult.inserted;
        skipped += cclResult.skipped;
        failed += cclResult.failed;

        stopwatch.Stop();
        _logger.LogInformation(
            "DollarRates ingestion summary for {Date}. Attempted={Attempted}, Inserted={Inserted}, Skipped={Skipped}, Failed={Failed}, DurationMs={DurationMs}",
            data.Date,
            attempted,
            inserted,
            skipped,
            failed,
            stopwatch.ElapsedMilliseconds);

        return new IngestionResult(data.Date, inserted, skipped, errors, details);
    }

    private async Task<(int inserted, int skipped, int failed)> ProcessRateAsync(
        DollarType dollarType,
        ExternalDollarRate rate,
        DateOnly date,
        bool forceManual,
        List<string> errors,
        List<IngestionDetail> details,
        CancellationToken cancellationToken)
    {
        var key = $"{dollarType}:{date:yyyy-MM-dd}";

        if (rate.Rate <= 0 || string.IsNullOrWhiteSpace(rate.Source))
        {
            details.Add(new IngestionDetail(key, "skipped", "Invalid rate or source"));
            return (0, 1, 0);
        }

        try
        {
            if (rate.Rate <= 100m || rate.Rate >= 10_000m)
            {
                _logger.LogWarning(
                    "Skipped invalid dollar rate {Type} {Date}. Rate={Rate}",
                    dollarType,
                    date,
                    rate.Rate);
                details.Add(new IngestionDetail(key, "skipped", "Invalid rate"));
                return (0, 1, 0);
            }

            var exists = await _dollarRateRepository.ExistsAsync(dollarType, date, rate.Source, cancellationToken);
            if (exists)
            {
                _logger.LogInformation("Skipped duplicate dollar rate {Type} {Date}", dollarType, date);
                details.Add(new IngestionDetail(key, "skipped", "Duplicate rate"));
                return (0, 1, 0);
            }

            var command = new UpsertDollarRateCommand(
                dollarType,
                rate.Rate,
                date,
                forceManual,
                rate.Source);
            await _mediator.Send(command, cancellationToken);
            details.Add(new IngestionDetail(key, "upserted", null));
            return (1, 0, 0);
        }
        catch (Exception ex)
        {
            errors.Add($"{key}: {ex.Message}");
            details.Add(new IngestionDetail(key, "error", ex.Message));
            return (0, 0, 1);
        }
    }
}
