using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CedearLedger.Infrastructure.Persistence.SqlServer;

public sealed class DollarRateConfiguration : IEntityTypeConfiguration<DollarRate>
{
    public void Configure(EntityTypeBuilder<DollarRate> builder)
    {
        builder.ToTable("DollarRates");

        builder.HasKey(rate => rate.Id);

        builder.Property(rate => rate.DollarType)
            .IsRequired();

        builder.Property(rate => rate.Rate)
            .HasPrecision(18, 6)
            .IsRequired();

        builder.Property(rate => rate.RateDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(rate => rate.IsManual)
            .IsRequired();

        builder.Property(rate => rate.Source)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(rate => rate.CreatedAt)
            .HasColumnType("datetime")
            .IsRequired();

        builder.HasIndex(rate => new { rate.DollarType, rate.RateDate })
            .IsUnique();
    }
}
