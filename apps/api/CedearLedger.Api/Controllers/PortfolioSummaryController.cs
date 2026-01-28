using CedearLedger.Domain.Models;
using CedearLedger.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace CedearLedger.Api.Controllers;

[ApiController]
[Route("api/portfolios")]
public sealed class PortfolioSummaryController : ControllerBase
{
    [HttpPost("summary")]
    public ActionResult<PortfolioSummary> PostSummary([FromBody] CalculationRequest request)
    {
        if (request is null || request.Operations is null || request.Operations.Count == 0)
        {
            return BadRequest();
        }

        if (request.CurrentPricesByTicker is null ||
            request.HistoricalMepByDate is null ||
            request.HistoricalCclByDate is null)
        {
            return BadRequest();
        }

        var engine = new CalculationEngine();
        var result = engine.Calculate(request);

        return Ok(result);
    }
}
