using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class CedearLedgerDbContext : DbContext
{
    public CedearLedgerDbContext(DbContextOptions<CedearLedgerDbContext> options)
        : base(options)
    {
    }

    public DbSet<DollarRate> DollarRates => Set<DollarRate>();
    public DbSet<CedearPrice> CedearPrices => Set<CedearPrice>();
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
        d => d.ToDateTime(TimeOnly.MinValue),
        d => DateOnly.FromDateTime(d)
        );

        modelBuilder.ApplyConfiguration(new DollarRateConfiguration());
        modelBuilder.ApplyConfiguration(new CedearPriceConfiguration());
        modelBuilder.ApplyConfiguration(new OperationConfiguration());
        modelBuilder.ApplyConfiguration(new PortfolioConfiguration());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateOnly))
                {
                    property.SetValueConverter(dateOnlyConverter);
                }
            }
        }
    }
}
