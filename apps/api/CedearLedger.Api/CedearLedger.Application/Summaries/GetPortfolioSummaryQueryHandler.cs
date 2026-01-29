using CedearLedger.Application.Abstractions;
using CedearLedger.Domain.Models;
using MediatR;

namespace CedearLedger.Application.Summaries;

public sealed class GetPortfolioSummaryQueryHandler : IRequestHandler<GetPortfolioSummaryQuery, PortfolioSummary?>
{
    private readonly IPortfolioSummaryQueryService _queryService;

    public GetPortfolioSummaryQueryHandler(IPortfolioSummaryQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<PortfolioSummary?> Handle(GetPortfolioSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _queryService.GetSummaryAsync(request.PortfolioId, cancellationToken);
    }
}
