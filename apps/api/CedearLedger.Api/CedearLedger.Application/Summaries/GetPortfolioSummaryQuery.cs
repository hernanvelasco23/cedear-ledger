using MediatR;
using CedearLedger.Domain.Models;

namespace CedearLedger.Application.Summaries;

public sealed record GetPortfolioSummaryQuery(Guid PortfolioId) : IRequest<PortfolioSummary?>;
