using CedearLedger.Application.Portfolios;
using CedearLedger.Application.Summaries;
using CedearLedger.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CedearLedger.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
public sealed class PortfoliosController : ControllerBase
{
    private readonly IMediator _mediator;

    public PortfoliosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<PortfolioDto>> PostAsync([FromBody] CreatePortfolioRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new CreatePortfolioCommand(request.Name), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PortfolioDto>>> GetAsync(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfoliosQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{portfolioId:guid}")]
    public async Task<ActionResult<PortfolioDto>> GetByIdAsync([FromRoute] Guid portfolioId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioByIdQuery(portfolioId), cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{portfolioId:guid}/summary")]
    public async Task<ActionResult<PortfolioSummary>> GetSummaryAsync([FromRoute] Guid portfolioId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioSummaryQuery(portfolioId), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("{portfolioId:guid}/valuation")]
    public async Task<ActionResult<PortfolioValuationDto>> GetValuationAsync([FromRoute] Guid portfolioId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(new GetPortfolioValuationQuery(portfolioId), cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (string.Equals(ex.Message, "Portfolio not found", StringComparison.Ordinal))
        {
            return NotFound();
        }
    }
}

public sealed record CreatePortfolioRequest(string Name);
