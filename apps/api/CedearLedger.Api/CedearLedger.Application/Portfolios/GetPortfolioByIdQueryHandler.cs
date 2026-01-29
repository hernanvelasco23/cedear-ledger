using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed class GetPortfolioByIdQueryHandler : IRequestHandler<GetPortfolioByIdQuery, PortfolioDto?>
{
    private readonly IPortfolioReadService _readService;

    public GetPortfolioByIdQueryHandler(IPortfolioReadService readService)
    {
        _readService = readService;
    }

    public async Task<PortfolioDto?> Handle(GetPortfolioByIdQuery request, CancellationToken cancellationToken)
    {
        return await _readService.GetPortfolioByIdAsync(request.PortfolioId, cancellationToken);
    }
}
