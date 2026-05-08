using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ServiceOrderProductLineConfiguration : IEntityTypeConfiguration<ServiceOrderProductLine>
{
    public void Configure(EntityTypeBuilder<ServiceOrderProductLine> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();
        builder.Property(l => l.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Quantity).IsRequired().HasPrecision(18, 3);
        builder.Property(l => l.UnitPrice).IsRequired().HasPrecision(18, 2);
        builder.Property(l => l.LineTotal).IsRequired().HasPrecision(18, 2);

        builder.HasOne(l => l.Product)
            .WithMany(p => p.ServiceOrderProductLines)
            .HasForeignKey(l => l.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_service_order_product_lines_quantity", "quantity > 0");
            t.HasCheckConstraint("ck_service_order_product_lines_unit_price", "unit_price >= 0");
            t.HasCheckConstraint("ck_service_order_product_lines_line_total", "line_total >= 0");
        });
    }
}
