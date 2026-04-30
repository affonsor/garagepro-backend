using GaragePro.Core.Entities;
using GaragePro.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GaragePro.Infrastructure.Data.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Type)
            .HasConversion(v => v.ToString(), v => Enum.Parse<AddressType>(v))
            .IsRequired();

        builder.Property(a => a.Street).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Number).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Complement).HasMaxLength(100);
        builder.Property(a => a.District).IsRequired().HasMaxLength(100);
        builder.Property(a => a.City).IsRequired().HasMaxLength(100);
        builder.Property(a => a.State).IsRequired().HasMaxLength(2);
        builder.Property(a => a.ZipCode).IsRequired().HasMaxLength(10);
        builder.Property(a => a.CreatedAt).IsRequired();
    }
}
