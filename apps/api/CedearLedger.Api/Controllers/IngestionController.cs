using CedearLedger.Application.Ingestion;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CedearLedger.Api.Controllers;

[ApiController]
[Route("api/ingestion")]
public sealed class IngestionController : ControllerBase
{
    private readonly IMediator _mediator;

    public IngestionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("dollar-rates")]
    public async Task<ActionResult<IngestionResult>> IngestDollarRatesAsync(
        [FromBody] IngestDollarRatesRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new IngestDollarRatesCommand(request.Date, request.ForceManual), cancellationToken);
        return Ok(result);
    }

    [HttpPost("cedear-prices")]
    public async Task<ActionResult<IngestionResult>> IngestCedearPricesAsync(
        [FromBody] IngestCedearPricesRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || request.Tickers is null || request.Tickers.Length == 0)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new IngestCedearPricesCommand(request.Date, request.Tickers, request.ForceManual), cancellationToken);
        return Ok(result);
    }
}

public sealed record IngestDollarRatesRequest(
    DateOnly Date,
    bool ForceManual
);

public sealed record IngestCedearPricesRequest(
    DateOnly Date,
    string[] Tickers,
    bool ForceManual
);
