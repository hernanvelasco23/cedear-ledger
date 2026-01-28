using Microsoft.EntityFrameworkCore;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class CedearLedgerDbContext : DbContext
{
    public CedearLedgerDbContext(DbContextOptions<CedearLedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<DollarRate> DollarRates => Set<DollarRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DollarRateConfiguration());
    }
}
