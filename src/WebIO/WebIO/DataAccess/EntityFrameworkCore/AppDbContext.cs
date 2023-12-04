namespace WebIO.DataAccess.EntityFrameworkCore;

using Entities;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<DeviceEntity>().HasIndex(d => d.Name).IsUnique();
    modelBuilder.Entity<InterfaceEntity>().HasIndex(d => new {d.DeviceId, d.Name}).IsUnique();
    modelBuilder.Entity<StreamEntity>().HasIndex(d => new {d.InterfaceId, d.Name}).IsUnique();

    modelBuilder.Entity<ChangeLogEntryEntity>().HasIndex(d => d.Timestamp);

    modelBuilder.Entity<DevicePropertyValueEntity>()
      .HasOne(p => p.Device)
      .WithMany(d => d.Properties)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<InterfacePropertyValueEntity>()
      .HasOne(p => p.Interface)
      .WithMany(d => d.Properties)
      .OnDelete(DeleteBehavior.Cascade);

    modelBuilder.Entity<StreamPropertyValueEntity>()
      .HasOne(p => p.Stream)
      .WithMany(d => d.Properties)
      .OnDelete(DeleteBehavior.Cascade);
  }

  public AppDbContext()
  {
  }

  public DbSet<AdminUser> AdminUsers { get; set; } = null!;

  public DbSet<DeviceEntity> Devices { get; set; } = null!;
  public DbSet<DeviceDenormalizedProperties> DevicesDenormalized { get; set; } = null!;

  public DbSet<InterfaceEntity> Interfaces { get; set; } = null!;
  public DbSet<InterfaceDenormalizedProperties> InterfacesDenormalized { get; set; } = null!;

  public DbSet<StreamEntity> Streams { get; set; } = null!;
  public DbSet<StreamPropertyValueEntity> StreamProperties { get; set; } = null!;

  public DbSet<ChangeLogEntryEntity> ChangeLog { get; set; } = null!;
}