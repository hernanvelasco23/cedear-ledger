using CedearLedger.Application.Ingestion;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CedearLedger.Infrastructure.BackgroundJobs;

public sealed class DollarRatesIngestionJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DollarRatesIngestionJob> _logger;
    private readonly IngestionOptions _options;

    public DollarRatesIngestionJob(
        IServiceProvider serviceProvider,
        ILogger<DollarRatesIngestionJob> logger,
        IOptions<IngestionOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("DollarRates ingestion job disabled.");
            return;
        }

        var job = _options.DollarRates ?? new JobOptions();
        var intervalMinutes = job.IntervalMinutes <= 0 ? 30 : job.IntervalMinutes;
        var delaySeconds = job.InitialDelaySeconds < 0 ? 0 : job.InitialDelaySeconds;
        var interval = TimeSpan.FromMinutes(intervalMinutes);
        var initialDelay = TimeSpan.FromSeconds(delaySeconds);
        _logger.LogInformation(
            "DollarRates ingestion job started. Interval: {Interval}. InitialDelay: {InitialDelay}",
            interval,
            initialDelay);

        await Task.Delay(initialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var date = DateOnly.FromDateTime(DateTime.UtcNow);

                _logger.LogInformation("Ingesting dollar rates for {Date}", date);
                await mediator.Send(new IngestDollarRatesCommand(date), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DollarRates ingestion failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
