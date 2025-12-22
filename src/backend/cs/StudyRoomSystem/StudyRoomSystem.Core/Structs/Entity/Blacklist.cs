namespace StudyRoomSystem.Core.Structs.Entity;

/// <summary>
/// 用户黑名单记录
/// </summary>
public record Blacklist
{
    public Guid Id { get; set; } = Ulid.NewUlid().ToGuid();
    public required Guid UserId { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public required DateTime ExpireTime { get; set; }
    public required string Type { get; set; }
    public required string Reason { get; set; }
    
    public virtual User User { get; set; } = null!;
}
