using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed class GetPortfolioValuationQueryHandler : IRequestHandler<GetPortfolioValuationQuery, PortfolioValuationDto>
{
    private readonly IPortfolioValuationQueryService _queryService;

    public GetPortfolioValuationQueryHandler(IPortfolioValuationQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PortfolioValuationDto> Handle(GetPortfolioValuationQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetValuationAsync(request.PortfolioId, cancellationToken);
    }
}
