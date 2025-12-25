using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

/// <summary>
/// 房间与座位服务接口，提供房间与座位的查询、创建、更新与删除操作。
/// </summary>
public interface IRoomService
{
    /// <summary>
    /// 根据座位 Id 获取座位信息
    /// </summary>
    Task<Seat> GetSeatById(Guid seatId);

    /// <summary>
    /// 根据房间 Id 获取房间信息
    /// </summary>
    Task<Room> GetRoomById(Guid roomId);

    /// <summary>
    /// 添加新的座位
    /// </summary>
    Task<Seat> AddSeat(Seat seat);

    /// <summary>
    /// 删除指定座位并返回所属房间信息
    /// </summary>
    Task<Room> DeleteSeat(Guid seatId);

    /// <summary>
    /// 分页获取所有房间
    /// </summary>
    Task<ApiPageResult<Room>> GetAllRoom(int page, int pageSize);

    /// <summary>
    /// 创建新的房间
    /// </summary>
    Task<Room> CreateRoom(Room room);

    /// <summary>
    /// 删除指定房间
    /// </summary>
    Task<Room> DeleteRoom(Guid roomId);

    /// <summary>
    /// 更新房间信息
    /// </summary>
    Task<Room> UpdateRoom(Room room);
}
