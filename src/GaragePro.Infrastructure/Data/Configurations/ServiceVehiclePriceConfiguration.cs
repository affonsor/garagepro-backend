using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ServiceVehiclePriceConfiguration : IEntityTypeConfiguration<ServiceVehiclePrice>
{
    public void Configure(EntityTypeBuilder<ServiceVehiclePrice> builder)
    {
        builder.HasKey(p => new { p.ServiceId, p.VehicleType });

        builder.Property(p => p.VehicleType)
            .HasConversion(v => v.ToString(), v => Enum.Parse<VehicleType>(v))
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Price).IsRequired().HasPrecision(18, 2);
        builder.ToTable(t => t.HasCheckConstraint("ck_service_vehicle_prices_price", "price >= 0"));

        builder.HasOne(p => p.Service)
            .WithMany(s => s.VehiclePrices)
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
