using GaragePro.Core.Entities;
using GaragePro.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace GaragePro.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<VehicleTransferRecord> VehicleTransferRecords => Set<VehicleTransferRecord>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentRescheduleHistory> AppointmentRescheduleHistories => Set<AppointmentRescheduleHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new ClientConfiguration());
        modelBuilder.ApplyConfiguration(new AddressConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleConfiguration());
        modelBuilder.ApplyConfiguration(new VehicleTransferRecordConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentRescheduleHistoryConfiguration());
    }
}
