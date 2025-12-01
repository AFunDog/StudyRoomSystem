using System.ComponentModel;

namespace StudyRoomSystem.Core.Structs;

public enum BookingStateEnum
{
    Booking,CheckIn,Checkout,Canceled
}

public class Booking
{
    [Description("编号")]
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required Guid SeatId { get; set; }
    public required DateTime CreateTime { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public DateTime? CheckInTime { get; set; }
    public DateTime? CheckOutTime { get; set; }
    public required BookingStateEnum State { get; set; } 
    
    
    public virtual User User { get; set; } = null!;
    public virtual Seat Seat { get; set; } = null!;
}
