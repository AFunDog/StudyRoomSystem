using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

public interface IBookingService
{
    Task<ApiPageResult<Booking>> GetAllByUserId(Guid userId, int page, int pageSize);
    Task<ApiPageResult<Booking>> GetAll(int page, int pageSize);
    Task<Booking> GetById(Guid bookingId);
    Task<Booking> Create(Booking booking);
    Task<Booking> Cancel(Guid bookingId,Guid userId,bool isForce = false);
    Task<Booking> CheckIn(Guid booingId,Guid userId);
    Task<Booking> CheckOut(Guid booingId,Guid userId);
}