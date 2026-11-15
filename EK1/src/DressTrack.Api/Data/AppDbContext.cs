using DressTrack.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace DressTrack.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private const int MaxNameLength = 200;

    public DbSet<User> Users => Set<User>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<PickupPoint> PickupPoints => Set<PickupPoint>();
    public DbSet<Parcel> Parcels => Set<Parcel>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Role>().HaveConversion<string>();
        configurationBuilder.Properties<ParcelStatus>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasIndex(u => u.Login).IsUnique();
            user.Property(u => u.Login).HasMaxLength(MaxNameLength);
            user.HasOne<Store>().WithMany().HasForeignKey(u => u.StoreId);
            user.HasOne<PickupPoint>().WithMany().HasForeignKey(u => u.PickupPointId);
        });

        modelBuilder.Entity<Parcel>(parcel =>
        {
            parcel.HasIndex(p => p.TrackingNumber).IsUnique();
            parcel.HasOne<Store>().WithMany().HasForeignKey(p => p.StoreId);
            parcel.HasOne<User>().WithMany().HasForeignKey(p => p.RecipientId);
            parcel.HasOne<PickupPoint>().WithMany().HasForeignKey(p => p.PickupPointId);
            parcel.HasMany(p => p.Items).WithOne().HasForeignKey("ParcelId");
            parcel.HasMany(p => p.History).WithOne().HasForeignKey("ParcelId");
        });

        modelBuilder.Entity<ParcelItem>().Property(i => i.Price).HasPrecision(10, 2);
        modelBuilder.Entity<ParcelStatusChange>().ToTable("ParcelStatusHistory");
    }
}
