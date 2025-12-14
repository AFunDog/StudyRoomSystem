using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs.Api.V2;

public class GetRoomResponseOk
{
    public required Room Room { get; set; }
    public Guid[]? Seats { get; set; }
}