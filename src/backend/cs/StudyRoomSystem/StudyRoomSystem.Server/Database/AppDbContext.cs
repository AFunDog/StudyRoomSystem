using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;

namespace StudyRoomSystem.Server.Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seat 关联 Room
        modelBuilder
            .Entity<Seat>()
            .HasOne(s => s.Room)
            .WithMany(r => r.Seats)
            .HasForeignKey(s => s.RoomId)
            .HasPrincipalKey(r => r.Id)
            .IsRequired();
        // Booking 关联 User
        modelBuilder
            .Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired();
        // Booking 关联 Seat
        modelBuilder
            .Entity<Booking>()
            .HasOne(b => b.Seat)
            .WithMany()
            .HasForeignKey(b => b.SeatId)
            .HasPrincipalKey(s => s.Id)
            .IsRequired();
        // Booking State 枚举转换
        modelBuilder.Entity<Booking>().Property(b => b.State).HasConversion<string>();
    }
}