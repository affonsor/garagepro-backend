using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Code).HasMaxLength(80);
        builder.HasIndex(s => s.Code).IsUnique().HasFilter("code IS NOT NULL");
        builder.Property(s => s.Tier).IsRequired().HasMaxLength(40).HasDefaultValue("standard");
        builder.Property(s => s.Category).HasMaxLength(120);
        builder.Property(s => s.Duration).HasMaxLength(40);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.Cost).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(s => s.IsActive).IsRequired().HasDefaultValue(true);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_services_price", "price >= 0");
            t.HasCheckConstraint("ck_services_cost", "cost >= 0");
        });

        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt).IsRequired();
    }
}
