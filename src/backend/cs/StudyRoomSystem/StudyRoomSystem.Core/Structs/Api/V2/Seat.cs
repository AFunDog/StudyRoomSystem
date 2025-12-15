using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs.Api.V2;

public class GetSeatResponseOk
{
    public required Seat Seat { get; set; }
    public KeyValuePair<DateTime,DateTime>[]? OpenTimes { get; set; }
}
