using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class AppointmentRescheduleHistoryConfiguration : IEntityTypeConfiguration<AppointmentRescheduleHistory>
{
    public void Configure(EntityTypeBuilder<AppointmentRescheduleHistory> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.Reason).HasMaxLength(2000);
        builder.Property(h => h.PreviousStartAt).IsRequired();
        builder.Property(h => h.PreviousExpectedEndAt).IsRequired();
        builder.Property(h => h.NewStartAt).IsRequired();
        builder.Property(h => h.NewExpectedEndAt).IsRequired();
        builder.Property(h => h.ChangedAt).IsRequired();

        builder.HasOne(h => h.Appointment)
            .WithMany(a => a.RescheduleHistory)
            .HasForeignKey(h => h.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedBy)
            .WithMany()
            .HasForeignKey(h => h.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
