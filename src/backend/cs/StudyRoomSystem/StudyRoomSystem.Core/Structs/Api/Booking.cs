namespace StudyRoomSystem.Core.Structs.Api;

public class CreateBookingRequest
{
    public required Guid SeatId { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
}


public class EditBookingRequest
{
    public required Guid Id { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}


public class CheckInRequest
{
    public Guid Id { get; set; }
}
public class CheckOutRequest
{
    public Guid Id { get; set; }
}