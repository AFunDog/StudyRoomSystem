using System.ComponentModel.DataAnnotations;

namespace StudyRoomSystem.Core.Structs.Api.V1;

public record CreateComplaintRequest
{
    public required Guid SeatId { get; set; }
    [MaxLength(64)]
    public required string Type { get; set; }
    [MaxLength(2048)]
    public required string Content { get; set; }
    public DateTime? TargetTime { get; set; }
}

public record EditComplaintRequest
{
    public required Guid Id { get; set; }
    [MaxLength(64)]
    public string? Type { get; set; }
    [MaxLength(2048)]
    public string? Content { get; set; }
    public DateTime? TargetTime { get; set; }
}

public record HandleComplaintRequest
{
    public required Guid Id { get; set; }
    public required string Content { get; set; }
}
public record CloseComplaintRequest
{
    public required Guid Id { get; set; }
}