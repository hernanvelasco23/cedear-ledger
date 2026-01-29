using CedearLedger.Application.Abstractions;
using CedearLedger.Application.DollarRates;
using CedearLedger.Application.Ingestion;
using CedearLedger.Domain.Models;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CedearLedger.Tests.Ingestion;

public sealed class IngestDollarRatesCommandHandlerTests
{
    [Fact]
    public async Task Handler_Upserts_Mep_And_Ccl()
    {
        var provider = new FakeMarketDataProvider(new ExternalDollarRatesResult(
            new DateOnly(2026, 1, 28),
            new ExternalDollarRate(1000.5m, "SRC"),
            new ExternalDollarRate(1100.75m, "SRC2")));

        var mediator = new RecordingMediator();

        var repository = new FakeDollarRateRepository();
        var handler = new IngestDollarRatesCommandHandler(provider, mediator, repository, NullLogger<IngestDollarRatesCommandHandler>.Instance);

        var result = await handler.Handle(new IngestDollarRatesCommand(new DateOnly(2026, 1, 28)), CancellationToken.None);

        Assert.Equal(2, result.InsertedOrUpdatedCount);
        Assert.Equal(2, mediator.Commands.Count);
        Assert.Contains(mediator.Commands, cmd => cmd.DollarType == DollarType.Mep && cmd.Rate == 1000.5m);
        Assert.Contains(mediator.Commands, cmd => cmd.DollarType == DollarType.Ccl && cmd.Rate == 1100.75m);
    }

    private sealed class FakeMarketDataProvider : IMarketDataProvider
    {
        private readonly ExternalDollarRatesResult _rates;

        public FakeMarketDataProvider(ExternalDollarRatesResult rates)
        {
            _rates = rates;
        }

        public Task<ExternalDollarRatesResult> GetDollarRatesAsync(DateOnly date, CancellationToken cancellationToken)
        {
            return Task.FromResult(_rates);
        }

        public Task<ExternalCedearPricesResult> GetCedearPricesAsync(DateOnly date, IReadOnlyList<string> tickers, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class RecordingMediator : IMediator
    {
        public List<UpsertDollarRateCommand> Commands { get; } = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is UpsertDollarRateCommand command)
            {
                Commands.Add(command);
            }

            return Task.FromResult(default(TResponse)!);
        }

        public Task Publish(object notification, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeDollarRateRepository : IDollarRateRepository
    {
        public Task<bool> ExistsAsync(DollarType dollarType, DateOnly rateDate, string source, CancellationToken cancellationToken)
        {
            return Task.FromResult(false);
        }

        public Task<DollarRateDto> UpsertAsync(UpsertDollarRateData data, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
