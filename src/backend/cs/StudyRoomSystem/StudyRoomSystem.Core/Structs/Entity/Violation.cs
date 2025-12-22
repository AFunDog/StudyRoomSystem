using System.Text.Json.Serialization;

namespace StudyRoomSystem.Core.Structs.Entity;

[JsonConverter(typeof(JsonStringEnumConverter<ViolationStateEnum>))]
public enum ViolationStateEnum
{
    Violation
}

[JsonConverter(typeof(JsonStringEnumConverter<ViolationTypeEnum>))]
public enum ViolationTypeEnum
{
    超时,
    强制取消,
    管理员
}

/// <summary>
/// 违规记录
/// </summary>
public class Violation
{
    public required Guid Id { get; set; }

    public Guid? BookingId { get; set; }
    public required Guid UserId { get; set; }
    public required DateTime CreateTime { get; set; }
    public required ViolationStateEnum State { get; set; }
    public required ViolationTypeEnum Type { get; set; }
    public required string Content { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Booking? Booking { get; set; }
}