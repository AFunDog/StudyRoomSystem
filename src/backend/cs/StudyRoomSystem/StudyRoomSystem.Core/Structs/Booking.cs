namespace StudyRoomSystem.Core.Structs;

public class Booking
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid SeatId { get; set; }
    public required DateTime CreateTime { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    
    public virtual User User { get; set; } = null!;
    public virtual Seat Seat { get; set; } = null!;
}
