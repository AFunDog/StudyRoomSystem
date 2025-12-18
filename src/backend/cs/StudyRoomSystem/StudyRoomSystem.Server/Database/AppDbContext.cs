using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Database;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Seat> Seats { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Violation> Violations { get; set; }
    public DbSet<Complaint> Complaints { get; set; }
    public DbSet<Blacklist> Blacklists { get; set; }


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region Users

        // User Role 枚举转换
        modelBuilder.Entity<User>().Property(u => u.Role).HasConversion<string>();

        #endregion

        #region Seats

        // Seat 关联 Room
        modelBuilder
            .Entity<Seat>()
            .HasOne(s => s.Room)
            .WithMany(r => r.Seats)
            .HasForeignKey(s => s.RoomId)
            .HasPrincipalKey(r => r.Id)
            .IsRequired();

        #endregion

        #region Bookings

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

        #endregion

        #region Violations

        // Violation 关联 User
        modelBuilder
            .Entity<Violation>()
            .HasOne(v => v.User)
            .WithMany()
            .HasForeignKey(v => v.UserId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired();
        // Violation 关联 Booking
        modelBuilder
            .Entity<Violation>()
            .HasOne(v => v.Booking)
            .WithMany()
            .HasForeignKey(v => v.BookingId)
            .HasPrincipalKey(u => u.Id)
            .IsRequired();
        // Violation State 枚举转换
        modelBuilder.Entity<Violation>().Property(v => v.State).HasConversion<string>();
        // Violation Type 枚举转换
        modelBuilder.Entity<Violation>().Property(v => v.Type).HasConversion<string>();

        #endregion

        #region Complaints

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
            .HasOne(c => c.Seat)
            .WithMany()
            .HasForeignKey(c => c.SeatId)
            .HasPrincipalKey(s => s.Id)
            .IsRequired();
        modelBuilder
            .Entity<Complaint>()
            .HasOne(c => c.HandleUser)
            .WithMany()
            .HasForeignKey(c => c.HandleUserId)
            .HasPrincipalKey(u => u.Id);
        // Complaint State 枚举转换
        modelBuilder.Entity<Complaint>().Property(c => c.State).HasConversion<string>();

        #endregion

        #region Blacklists
        // Blacklist 关联 User
        modelBuilder
            .Entity<Blacklist>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .HasPrincipalKey(u => u.Id);

        #endregion
    }
}