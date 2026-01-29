using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("Portfolios");

        builder.HasKey(portfolio => portfolio.Id);

        builder.Property(portfolio => portfolio.Name)
            .IsRequired();

        builder.Property(portfolio => portfolio.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();
    }
}
