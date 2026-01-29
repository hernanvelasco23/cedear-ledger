using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.ToTable("Operations");

        builder.HasKey(operation => operation.Id);

        builder.Property(operation => operation.PortfolioId)
            .IsRequired();

        builder.Property(operation => operation.Ticker)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(operation => operation.Quantity)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(operation => operation.PriceArs)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(operation => operation.FeesArs)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(operation => operation.OperationDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(operation => operation.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasOne<Portfolio>()
            .WithMany()
            .HasForeignKey(operation => operation.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(operation => operation.PortfolioId);
    }
}
