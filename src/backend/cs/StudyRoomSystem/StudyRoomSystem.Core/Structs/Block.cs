namespace StudyRoomSystem.Core.Structs;

/// <summary>
/// 用户锁定记录
/// </summary>
public class Block
{
    public required Guid Id { get; set; }
    public required Guid UserId { get; set; }
    public required DateTime CreateTime { get; set; }
    public required DateTime ExpireTime { get; set; }
    public required string Reason { get; set; }
    public required string Type { get; set; }
}
