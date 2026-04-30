using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
        builder.ToTable(t => t.HasCheckConstraint("ck_products_price", "price >= 0"));

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
