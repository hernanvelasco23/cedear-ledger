using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.Operations;

public sealed class GetPortfolioOperationsQueryHandler : IRequestHandler<GetPortfolioOperationsQuery, IReadOnlyList<OperationDto>?>
{
    private readonly IPortfolioReadService _readService;

    public GetPortfolioOperationsQueryHandler(IPortfolioReadService readService)
    {
        _readService = readService;
    }

    public async Task<IReadOnlyList<OperationDto>?> Handle(GetPortfolioOperationsQuery request, CancellationToken cancellationToken)
    {
        return await _readService.GetPortfolioOperationsAsync(request.PortfolioId, cancellationToken);
    }
}
