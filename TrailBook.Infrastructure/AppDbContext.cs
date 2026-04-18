using TrailBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace TrailBook.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Destination).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.OwnsOne(e => e.RefundPolicy);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PriceAtBooking).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.IdempotencyKey).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => e.State);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);

            entity.HasOne(e => e.Trip)
                .WithMany()
                .HasForeignKey(e => e.TripId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}