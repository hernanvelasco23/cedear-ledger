namespace CedearLedger.Infrastructure.BackgroundJobs;

public sealed class IngestionOptions
{
    public bool Enabled { get; set; } = true;
    public JobOptions DollarRates { get; set; } = new();
    public JobOptions CedearPrices { get; set; } = new();
}

public sealed class JobOptions
{
    public int IntervalMinutes { get; set; } = 30;
    public int InitialDelaySeconds { get; set; } = 10;
}
