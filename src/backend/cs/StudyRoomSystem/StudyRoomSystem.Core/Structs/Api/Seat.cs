using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs.Api;

public class GetSeatResponseOk
{
    public required Seat Seat { get; set; }
    public KeyValuePair<DateTime,DateTime>[]? OpenTimes { get; set; }
}
