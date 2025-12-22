using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs.Api.V1;

public record CreateViolationRequest
{
    public required Guid UserId { get; set; }
    public Guid? BookingId { get; set; }
    public required ViolationTypeEnum Type { get; set; }
    public required string Content { get; set; }
}
public record EditViolationRequest
{
    public required Guid Id { get; set; }
    public ViolationTypeEnum? Type { get; set; }
    public string? Content { get; set; }
}


