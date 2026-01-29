using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.Operations;

public sealed class CreateOperationCommandHandler : IRequestHandler<CreateOperationCommand, OperationDto?>
{
    private readonly IOperationRepository _repository;

    public CreateOperationCommandHandler(IOperationRepository repository)
    {
        _repository = repository;
    }

    public async Task<OperationDto?> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _repository.CreateAsync(new CreateOperationData(
                request.PortfolioId,
                request.Ticker,
                request.Quantity,
                request.PriceArs,
                request.FeesArs,
                request.OperationDate), cancellationToken);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }
}
