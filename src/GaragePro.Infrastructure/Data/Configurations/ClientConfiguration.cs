using GaragePro.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Email).HasMaxLength(256);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Document).HasMaxLength(20);

        builder.HasMany(c => c.Addresses)
            .WithOne()
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Vehicles)
            .WithOne(v => v.Client)
            .HasForeignKey(v => v.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();
    }
}
