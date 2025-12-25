using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

/// <summary>
/// 预约服务接口，提供预约的查询、创建、取消、签到、签退与删除操作。
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// 分页获取指定用户的预约记录
    /// </summary>
    Task<ApiPageResult<Booking>> GetAllByUserId(Guid userId, int page, int pageSize);

    /// <summary>
    /// 分页获取预约记录，可按房间、时间区间与状态筛选
    /// </summary>
    Task<ApiPageResult<Booking>> GetAll(
        int page,
        int pageSize,
        Guid? roomId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        BookingStateEnum? state = null);

    /// <summary>
    /// 根据 Id 获取单条预约记录
    /// </summary>
    Task<Booking> GetById(Guid bookingId);

    /// <summary>
    /// 创建新的预约
    /// </summary>
    Task<Booking> Create(Booking booking);

    /// <summary>
    /// 取消指定预约（支持强制取消）
    /// </summary>
    Task<Booking> Cancel(Guid bookingId, Guid userId, bool isForce = false);

    /// <summary>
    /// 签到操作，标记用户已到场
    /// </summary>
    Task<Booking> CheckIn(Guid booingId, Guid userId);

    /// <summary>
    /// 签退操作，标记用户已离开并结束预约
    /// </summary>
    Task<Booking> CheckOut(Guid booingId, Guid userId);

    /// <summary>
    /// 删除指定预约记录
    /// </summary>
    Task Delete(Guid bookingId);
}