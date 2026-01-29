using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed class GetPortfoliosQueryHandler : IRequestHandler<GetPortfoliosQuery, IReadOnlyList<PortfolioDto>>
{
    private readonly IPortfolioReadService _readService;

    public GetPortfoliosQueryHandler(IPortfolioReadService readService)
    {
        _readService = readService;
    }

    public async Task<IReadOnlyList<PortfolioDto>> Handle(GetPortfoliosQuery request, CancellationToken cancellationToken)
    {
        return await _readService.GetPortfoliosAsync(cancellationToken);
    }
}
