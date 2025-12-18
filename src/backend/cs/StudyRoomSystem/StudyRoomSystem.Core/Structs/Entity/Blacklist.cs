namespace StudyRoomSystem.Core.Structs.Entity;

/// <summary>
/// 用户黑名单记录
/// </summary>
public class Blacklist
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required DateTime CreateTime { get; set; }
    public required DateTime ExpireTime { get; set; }
    public required string Type { get; set; }
    public required string Reason { get; set; }
    
    public virtual User User { get; set; } = null!;
}
