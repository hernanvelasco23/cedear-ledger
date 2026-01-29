namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class Portfolio
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
