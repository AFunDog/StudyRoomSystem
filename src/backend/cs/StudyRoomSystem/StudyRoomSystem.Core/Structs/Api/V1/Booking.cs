using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs.Api.V1;

public record GetBookingFilterRequest
{
    public Guid? RoomId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public BookingStateEnum? State { get; set; }
}

public class CreateBookingRequest
{
    public required Guid SeatId { get; set; }
    // UTC ISO格式时间
    public required DateTime StartTime { get; set; }
    // UTC ISO格式时间
    public required DateTime EndTime { get; set; }
}


public class EditBookingRequest
{
    public required Guid Id { get; set; }
    // UTC ISO格式时间
    public DateTime? StartTime { get; set; }
    // UTC ISO格式时间
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