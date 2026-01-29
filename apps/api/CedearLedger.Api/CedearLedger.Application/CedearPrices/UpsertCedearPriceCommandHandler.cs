using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.CedearPrices;

public sealed class UpsertCedearPriceCommandHandler : IRequestHandler<UpsertCedearPriceCommand, CedearPriceDto>
{
    private readonly ICedearPriceRepository _repository;

    public UpsertCedearPriceCommandHandler(ICedearPriceRepository repository)
    {
        _repository = repository;
    }

    public async Task<CedearPriceDto> Handle(UpsertCedearPriceCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpsertAsync(new UpsertCedearPriceData(
            request.Ticker,
            request.PriceArs,
            request.PriceDate,
            request.IsManual,
            request.Source), cancellationToken);
    }
}
