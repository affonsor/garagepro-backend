using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ServiceStepConfiguration : IEntityTypeConfiguration<ServiceStep>
{
    public void Configure(EntityTypeBuilder<ServiceStep> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.Position).IsRequired();
        builder.Property(s => s.Description).IsRequired().HasMaxLength(500);

        builder.HasOne(s => s.Service)
            .WithMany(s => s.Steps)
            .HasForeignKey(s => s.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => new { s.ServiceId, s.Position }).IsUnique();
    }
}
