using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;

namespace StudyRoomSystem.Server.Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Violation> Violations { get; set; }
    public DbSet<Complaint> Complaints { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region 数据关联

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
            .WithMany(s => s.Bookings)
            .HasForeignKey(b => b.SeatId)
            .HasPrincipalKey(s => s.Id)
            .IsRequired();
        // Booking State 枚举转换
        modelBuilder.Entity<Booking>().Property(b => b.State).HasConversion<string>();
        // Violation 关联 User
        modelBuilder
            .Entity<Violation>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired();
        // Complaint 关联 User
        modelBuilder
            .Entity<Complaint>()
            .HasOne(c => c.SendUser)
            .WithMany()
            .HasForeignKey(c => c.SendUserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired();
        modelBuilder
            .Entity<Complaint>()
            .HasOne(c => c.ReceiveUser)
            .WithMany()
            .HasForeignKey(c => c.ReceiveUserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired();
        modelBuilder
            .Entity<Complaint>()
            .HasOne(c => c.HandleUser)
            .WithMany()
            .HasForeignKey(c => c.HandleUserId)
            .HasPrincipalKey(u => u.Id);

        #endregion
    }
}