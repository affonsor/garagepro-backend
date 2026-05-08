using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ServiceMaterialConfiguration : IEntityTypeConfiguration<ServiceMaterial>
{
    public void Configure(EntityTypeBuilder<ServiceMaterial> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Quantity).IsRequired().HasPrecision(18, 3);

        builder.HasOne(m => m.Service)
            .WithMany(s => s.Materials)
            .HasForeignKey(m => m.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Product)
            .WithMany(p => p.ServiceMaterials)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(m => new { m.ServiceId, m.ProductId }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint("ck_service_materials_quantity", "quantity > 0"));
    }
}
