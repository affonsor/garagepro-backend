using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ServiceOrderConfiguration : IEntityTypeConfiguration<ServiceOrder>
{
    public void Configure(EntityTypeBuilder<ServiceOrder> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.Property(o => o.BoxNumber).IsRequired();
        builder.Property(o => o.Status)
            .HasConversion(v => v.ToString(), v => Enum.Parse<OrderStatus>(v))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(o => o.TotalPrice).IsRequired().HasPrecision(18, 2);
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.ScheduledAt).IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
        builder.Property(o => o.UpdatedAt).IsRequired();

        builder.HasOne(o => o.Client)
            .WithMany(c => c.ServiceOrders)
            .HasForeignKey(o => o.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Vehicle)
            .WithMany(v => v.ServiceOrders)
            .HasForeignKey(o => o.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.ServiceLines)
            .WithOne(l => l.ServiceOrder)
            .HasForeignKey(l => l.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.ProductLines)
            .WithOne(l => l.ServiceOrder)
            .HasForeignKey(l => l.ServiceOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.ScheduledAt);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.ClientId);
        builder.HasIndex(o => o.VehicleId);
        builder.ToTable(t =>
        {
            t.HasCheckConstraint("ck_service_orders_box_number", "box_number between 1 and 6");
            t.HasCheckConstraint("ck_service_orders_total_price", "total_price >= 0");
        });
    }
}
