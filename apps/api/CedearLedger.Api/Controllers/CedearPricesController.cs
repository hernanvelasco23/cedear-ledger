using CedearLedger.Application.CedearPrices;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CedearLedger.Api.Controllers;

[ApiController]
[Route("api/cedear-prices")]
public sealed class CedearPricesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CedearPricesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<CedearPriceDto>> PostAsync([FromBody] UpsertCedearPriceRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Ticker) || string.IsNullOrWhiteSpace(request.Source))
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new UpsertCedearPriceCommand(
            request.Ticker,
            request.PriceArs,
            request.PriceDate,
            request.IsManual,
            request.Source), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}

public sealed record UpsertCedearPriceRequest(
    string Ticker,
    decimal PriceArs,
    DateOnly PriceDate,
    bool IsManual,
    string Source
);
