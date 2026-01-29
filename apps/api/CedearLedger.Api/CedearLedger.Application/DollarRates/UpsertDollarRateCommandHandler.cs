using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.DollarRates;

public sealed class UpsertDollarRateCommandHandler : IRequestHandler<UpsertDollarRateCommand, DollarRateDto>
{
    private readonly IDollarRateRepository _repository;

    public UpsertDollarRateCommandHandler(IDollarRateRepository repository)
    {
        _repository = repository;
    }

    public async Task<DollarRateDto> Handle(UpsertDollarRateCommand request, CancellationToken cancellationToken)
    {
        return await _repository.UpsertAsync(new UpsertDollarRateData(
            request.DollarType,
            request.Rate,
            request.RateDate,
            request.IsManual,
            request.Source), cancellationToken);
    }
}
