using CedearLedger.Infrastructure.Persistence.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CedearLedger.Api.Controllers;

[ApiController]
[Route("api/dollar-rates")]
public sealed class DollarRatesController : ControllerBase
{
    private readonly CedearLedgerDbContext _dbContext;

    public DollarRatesController(CedearLedgerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<ActionResult<DollarRate>> PostAsync([FromBody] CreateDollarRateRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return BadRequest();
        }

        if (request.Rate <= 0)
        {
            return BadRequest();
        }
        
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            return BadRequest();
        }

        var existing = await _dbContext.DollarRates
            .FirstOrDefaultAsync(
                rate => rate.DollarType == request.DollarType && rate.RateDate == request.RateDate,
                cancellationToken);

        if (existing is null)
        {
            existing = new DollarRate
            {
                Id = Guid.NewGuid(),
                DollarType = request.DollarType,
                Rate = request.Rate,
                RateDate = request.RateDate,
                IsManual = request.IsManual,
                Source = request.Source,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.DollarRates.Add(existing);
        }
        else
        {
            existing.Rate = request.Rate;
            existing.IsManual = request.IsManual;
            existing.Source = request.Source;
            existing.CreatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return StatusCode(StatusCodes.Status201Created, existing);
    }
}

public sealed record CreateDollarRateRequest(
    DollarType DollarType,
    decimal Rate,
    DateOnly RateDate,
    bool IsManual,
    string Source
);
