namespace StudyRoomSystem.Core.Structs.Entity;

/// <summary>
/// 违规记录
/// </summary>
public class Violation
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required string State { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    
    public virtual User User { get; set; } = null!;
}
