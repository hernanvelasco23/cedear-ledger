using CedearLedger.Application.Operations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CedearLedger.Api.Controllers;

[ApiController]
[Route("api/portfolios/{portfolioId:guid}/operations")]
public sealed class PortfolioOperationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PortfolioOperationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<OperationDto>> PostAsync(
        [FromRoute] Guid portfolioId,
        [FromBody] CreateOperationRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Ticker))
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new CreateOperationCommand(
            portfolioId,
            request.Ticker,
            request.Quantity,
            request.PriceArs,
            request.FeesArs,
            request.OperationDate), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<OperationDto>>> GetAsync(
        [FromRoute] Guid portfolioId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioOperationsQuery(portfolioId), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}

public sealed record CreateOperationRequest(
    string Ticker,
    decimal Quantity,
    decimal PriceArs,
    decimal FeesArs,
    DateOnly OperationDate
);
