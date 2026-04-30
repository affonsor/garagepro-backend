using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class VehicleTransferRecordConfiguration : IEntityTypeConfiguration<VehicleTransferRecord>
{
    public void Configure(EntityTypeBuilder<VehicleTransferRecord> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Notes).HasMaxLength(500);
        builder.Property(t => t.TransferredAt).IsRequired();

        builder.HasOne(t => t.FromClient)
            .WithMany()
            .HasForeignKey(t => t.FromClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ToClient)
            .WithMany()
            .HasForeignKey(t => t.ToClientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
