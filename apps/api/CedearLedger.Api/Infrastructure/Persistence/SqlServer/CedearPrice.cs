namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class CedearPrice
{
    public Guid Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public decimal PriceArs { get; set; }
    public DateOnly PriceDate { get; set; }
    public bool IsManual { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
