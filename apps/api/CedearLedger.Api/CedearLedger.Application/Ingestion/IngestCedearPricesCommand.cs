using MediatR;

namespace CedearLedger.Application.Ingestion;

public sealed record IngestCedearPricesCommand(
    DateOnly Date,
    string[] Tickers,
    bool ForceManualFlag = false
) : IRequest<IngestionResult>;
