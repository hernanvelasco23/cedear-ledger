using CedearLedger.Application.DollarRates;
using CedearLedger.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CedearLedger.Api.Controllers;

[ApiController]
[Route("api/dollar-rates")]
public sealed class DollarRatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DollarRatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<DollarRateDto>> PostAsync([FromBody] UpsertDollarRateRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(request.Source))
        {
            return BadRequest();
        }

        if (request.Rate <= 0)
        {
            return BadRequest();
        }

        var result = await _mediator.Send(new UpsertDollarRateCommand(
            request.DollarType,
            request.Rate,
            request.RateDate,
            request.IsManual,
            request.Source), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}

public sealed record UpsertDollarRateRequest(
    DollarType DollarType,
    decimal Rate,
    DateOnly RateDate,
    bool IsManual,
    string Source
);
