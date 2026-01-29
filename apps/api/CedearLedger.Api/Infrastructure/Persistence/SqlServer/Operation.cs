namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class Operation
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal PriceArs { get; set; }
    public decimal FeesArs { get; set; }
    public DateOnly OperationDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
