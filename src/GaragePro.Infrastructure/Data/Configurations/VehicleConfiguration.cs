using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedNever();

        builder.Property(v => v.LicensePlate).IsRequired().HasMaxLength(20);
        builder.HasIndex(v => v.LicensePlate).IsUnique();

        builder.Property(v => v.Make).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Model).IsRequired().HasMaxLength(100);
        builder.Property(v => v.Year).IsRequired();
        builder.Property(v => v.Color).HasMaxLength(50);
        builder.Property(v => v.VIN).HasMaxLength(17);

        builder.HasOne(v => v.Client)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(v => v.TransferHistory)
            .WithOne()
            .HasForeignKey(t => t.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(v => v.CreatedAt).IsRequired();
        builder.Property(v => v.UpdatedAt).IsRequired();
    }
}
