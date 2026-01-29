namespace CedearLedger.Application.Ingestion;

public sealed record IngestionResult(
    DateOnly Date,
    int InsertedOrUpdatedCount,
    int SkippedCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<IngestionDetail> Details
);

public sealed record IngestionDetail(
    string Key,
    string Status,
    string? Message
);
