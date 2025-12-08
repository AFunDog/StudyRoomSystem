namespace StudyRoomSystem.Core.Structs.Entity;

/// <summary>
/// 投诉记录
/// </summary>

public class Complaint
{
    public required Guid Id { get; set; }
    public required Guid SendUserId { get; set; }
    public required Guid ReceiveUserId { get; set; }
    public required string State { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    public required DateTime CreateTime { get; set; }
    public DateTime? HandleTime { get; set; }
    public Guid? HandleUserId { get; set; }
    
    public virtual User SendUser { get; set; } = null!;
    public virtual User ReceiveUser { get; set; } = null!;
    public virtual User? HandleUser { get; set; }
}
