using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.ProductValueSnapshot).IsRequired().HasPrecision(18, 2);
        builder.Property(a => a.ServiceValueSnapshot).IsRequired().HasPrecision(18, 2);
        builder.Property(a => a.TotalValue).IsRequired().HasPrecision(18, 2);
        builder.Property(a => a.Notes).HasMaxLength(2000);
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.StartAt).IsRequired();
        builder.Property(a => a.ExpectedEndAt).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasOne(a => a.Client).WithMany().HasForeignKey(a => a.ClientId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Product).WithMany().HasForeignKey(a => a.ProductId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Service).WithMany().HasForeignKey(a => a.ServiceId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => a.StartAt);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.ClientId);

        // Optimistic concurrency via PostgreSQL xmin system column
        builder.Property<uint>("xmin").HasColumnType("xid").ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();
    }
}
