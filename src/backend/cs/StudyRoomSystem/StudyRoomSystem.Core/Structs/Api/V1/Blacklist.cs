namespace StudyRoomSystem.Core.Structs.Api.V1;

public record CreateBlacklistRequest
{
    public required Guid UserId { get; set; }
    public required DateTime ExpireTime { get; set; }
    public required string Reason { get; set; }
    public required string Type { get; set; }
}
public record EditBlacklistRequest
{
    public required Guid Id { get; set; }
    public DateTime? ExpireTime { get; set; }
    public string? Reason { get; set; }
    public string? Type { get; set; }
}

