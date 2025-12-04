using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs;

public class Seat
{
    public required Guid Id { get; set; }
    public required Guid RoomId { get; set; }
    public required int Row { get; set; }
    public required int Col { get; set; }

    public virtual Room Room { get; set; } = null!;
    
    public virtual ICollection<Booking> Bookings { get; set; } = [];
}
