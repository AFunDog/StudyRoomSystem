using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

public interface IRoomService
{
    Task<Seat> GetSeatById(Guid seatId);
    Task<Room> GetRoomById(Guid roomId);
    Task<Seat> AddSeat(Seat seat);
    Task<Room> DeleteSeat(Guid seatId);
    Task<ApiPageResult<Room>> GetAllRoom(int page, int pageSize);
    Task<Room> CreateRoom(Room room);
    Task<Room> DeleteRoom(Guid roomId);
    Task<Room> UpdateRoom(Room room);
}
