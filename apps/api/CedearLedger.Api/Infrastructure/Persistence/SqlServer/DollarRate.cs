namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class DollarRate
{
    public Guid Id { get; set; }
    public DollarType DollarType { get; set; }
    public decimal Rate { get; set; }
    public DateOnly RateDate { get; set; }
    public bool IsManual { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
