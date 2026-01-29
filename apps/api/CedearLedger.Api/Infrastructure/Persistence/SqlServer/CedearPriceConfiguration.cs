using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class CedearPriceConfiguration : IEntityTypeConfiguration<CedearPrice>
{
    public void Configure(EntityTypeBuilder<CedearPrice> builder)
    {
        builder.ToTable("CedearPrices");

        builder.HasKey(price => price.Id);

        builder.Property(price => price.Ticker)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(price => price.PriceArs)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(price => price.PriceDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(price => price.IsManual)
            .IsRequired();

        builder.Property(price => price.Source)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(price => price.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasIndex(price => new { price.Ticker, price.PriceDate })
            .IsUnique();
    }
}
