using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.Property(u => u.PasswordHash).IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        var rolesComparer = new ValueComparer<List<UserRole>>(
            (a, b) => a != null && b != null && a.SequenceEqual(b),
            v => v.Aggregate(0, (h, r) => HashCode.Combine(h, r.GetHashCode())),
            v => v.ToList());

        builder.Property(u => u.Roles)
            .HasConversion(
                v => v.Select(r => r.ToString()).ToArray(),
                v => v.Select(Enum.Parse<UserRole>).ToList())
            .HasColumnType("text[]")
            .Metadata.SetValueComparer(rolesComparer);

        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.UpdatedAt).IsRequired();
    }
}
