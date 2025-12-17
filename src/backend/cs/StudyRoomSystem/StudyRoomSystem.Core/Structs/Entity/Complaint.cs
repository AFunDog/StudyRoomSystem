using System.Text.Json.Serialization;

namespace StudyRoomSystem.Core.Structs.Entity;

[JsonConverter(typeof(JsonStringEnumConverter<ComplaintStateEnum>))]
public enum ComplaintStateEnum
{
    无, 已发起, 已处理, 已关闭
}

/// <summary>
/// 投诉记录
/// </summary>
public class Complaint
{
    public required Guid Id { get; set; }
    public required Guid SendUserId { get; set; }
    public required Guid ReceiveUserId { get; set; }
    public required ComplaintStateEnum State { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    public required DateTime CreateTime { get; set; }
    public DateTime? HandleTime { get; set; }
    public Guid? HandleUserId { get; set; }

    public virtual User SendUser { get; set; } = null!;
    public virtual User ReceiveUser { get; set; } = null!;
    public virtual User? HandleUser { get; set; }
}