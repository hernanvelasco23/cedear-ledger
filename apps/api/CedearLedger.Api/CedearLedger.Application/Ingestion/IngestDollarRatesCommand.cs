using MediatR;

namespace CedearLedger.Application.Ingestion;

public sealed record IngestDollarRatesCommand(
    DateOnly Date,
    bool ForceManualFlag = false
) : IRequest<IngestionResult>;
