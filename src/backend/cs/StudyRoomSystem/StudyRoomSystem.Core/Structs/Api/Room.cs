using System.ComponentModel.DataAnnotations;

namespace StudyRoomSystem.Core.Structs.Api;

public class CreateRoomRequest
{
    public required string Name { get; set; }
    public required TimeOnly OpenTime { get; set; }
    public required TimeOnly CloseTime { get; set; }

    [Range(1, 2048)]
    public required int Rows { get; set; }
    [Range(1, 2048)]
    public required int Cols { get; set; }
}

public class EditRoomRequest
{
    public required Guid Id { get; set; }
    public string? Name { get; set; }
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }

    [Range(1, 2048)]
    public int? Rows { get; set; }
    [Range(1, 2048)]
    public int? Cols { get; set; }
}
