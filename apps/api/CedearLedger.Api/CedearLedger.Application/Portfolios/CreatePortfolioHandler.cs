using CedearLedger.Application.Abstractions;
using MediatR;

namespace CedearLedger.Application.Portfolios;

public sealed class CreatePortfolioHandler : IRequestHandler<CreatePortfolioCommand, PortfolioDto>
{
    private readonly IPortfolioRepository _repository;

    public CreatePortfolioHandler(IPortfolioRepository repository)
    {
        _repository = repository;
    }

    public async Task<PortfolioDto> Handle(CreatePortfolioCommand request, CancellationToken cancellationToken)
    {
        return await _repository.CreateAsync(request.Name, cancellationToken);
    }
}
