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
        builder.Property(p => p.Sku).HasMaxLength(80);
        builder.HasIndex(p => p.Sku).IsUnique().HasFilter("sku IS NOT NULL");
        builder.Property(p => p.Brand).HasMaxLength(120);
        builder.Property(p => p.Category).HasMaxLength(120);
        builder.Property(p => p.Size).HasMaxLength(40);
        builder.Property(p => p.Unit).IsRequired().HasMaxLength(20).HasDefaultValue("un");
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Cost).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.Price).IsRequired().HasPrecision(18, 2);
        builder.Property(p => p.Stock).IsRequired().HasPrecision(18, 3);
        builder.Property(p => p.MinStock).IsRequired().HasPrecision(18, 3);
        builder.Property(p => p.Supplier).HasMaxLength(160);
        builder.Property(p => p.Barcode).HasMaxLength(80);
        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_products_price", "price >= 0");
            t.HasCheckConstraint("ck_products_cost", "cost >= 0");
            t.HasCheckConstraint("ck_products_stock", "stock >= 0");
            t.HasCheckConstraint("ck_products_min_stock", "min_stock >= 0");
        });

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt).IsRequired();
    }
}
