using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Helpers;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Services;

internal sealed class BookingService(AppDbContext appDbContext, IUserService userService) : IBookingService
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IUserService UserService { get; } = userService;

    public async Task<ApiPageResult<Booking>> GetAllByUserId(Guid userId, int page, int pageSize)
    {
        return await AppDbContext
            .Bookings.AsNoTracking()
            .Include(x => x.Seat)
            .Include(x => x.Seat.Room)
            .OrderByDescending(x => x.CreateTime)
            .Where(x => x.UserId == userId)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<ApiPageResult<Booking>> GetAll(
        int page,
        int pageSize,
        Guid? roomId = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        BookingStateEnum? state = null)
    {
        IQueryable<Booking> query = AppDbContext
            .Bookings.AsNoTracking()
            .Include(x => x.Seat)
            .Include(x => x.Seat.Room)
            .OrderByDescending(x => x.CreateTime);

        if (roomId is not null)
            query = query.Where(x => x.Seat.Room.Id == roomId);

        if (startTime is not null)
            query = query.Where(x => startTime <= x.EndTime);

        if (endTime is not null)
            query = query.Where(x => x.StartTime <= endTime);

        return await query.ToApiPageResult(page, pageSize);
    }


    public async Task<Booking> GetById(Guid bookingId)
    {
        var booking = await AppDbContext.Bookings.SingleOrDefaultAsync(x => x.Id == bookingId);
        if (booking is null)
            throw new NotFoundException("预约不存在");
        return booking;
    }

    public async Task<Booking> Create(Booking booking)
    {
        // 检查座位
        var seat = await AppDbContext
            .Seats.AsNoTracking()
            .Include(x => x.Room)
            .SingleOrDefaultAsync(x => x.Id == booking.SeatId);
        if (seat is null)
            throw new NotFoundException("座位不存在");

        // 检查输入时间是否合法
        if (booking.StartTime >= booking.EndTime)
            throw new BadHttpRequestException("开始时间不能大于结束时间");

        // 不允许创建过去的预约
        if (booking.StartTime <= DateTime.UtcNow)
            throw new BadHttpRequestException("不允许创建过去的预约");

        // 预约不允许超过房间开放时间
        var start = booking.StartTime.ToLocalTime();
        var end = booking.EndTime.ToLocalTime();
        if (!(start.Date.Add(seat.Room.OpenTime.ToTimeSpan()) <= start
              && end <= start.Date.Add(seat.Room.CloseTime.ToTimeSpan())))
        {
            throw new BadHttpRequestException("不允许超过房间开放时间");
        }

        // 检查座位是否在时间段内被占用
        var has = await AppDbContext.Bookings.AnyAsync(x
            => x.SeatId == booking.SeatId
               && ((x.StartTime <= booking.StartTime && booking.StartTime <= x.EndTime)
                   || (x.StartTime <= booking.EndTime && booking.EndTime <= x.EndTime))
               && (x.State == (BookingStateEnum.已预约) || x.State == (BookingStateEnum.已签到))
        );
        if (has)
            throw new ConflictException("座位在时间范围内已被用户预约");

        // 检查用户最长预约时间
        var user = await UserService.GetUserById(booking.UserId);
        if (user.GetBookingLimit() < booking.EndTime - booking.StartTime)
            throw new BadHttpRequestException("用户预约时间超过限制");

        var track = await AppDbContext.Bookings.AddAsync(booking);
        await AppDbContext.SaveChangesAsync();
        return track.Entity;
    }

    public async Task<Booking> Cancel(Guid bookingId, Guid userId, bool isForce = false)
    {
        var booking = await GetById(bookingId);
        var user = await UserService.GetUserById(userId);
        if (user.Role is not UserRoleEnum.Admin && booking.UserId != user.Id)
            throw new ForbidException("没有权限");
        if (booking.State is not BookingStateEnum.已预约)
            throw new BadHttpRequestException("必须已预约的才能取消预约");

        if (booking.StartTime - DateTime.UtcNow < TimeSpan.FromHours(3))
            if (!isForce)
                throw new BadHttpRequestException("距离预约起始时间小于3小时，若强制取消预约将会记录为违规");
            else
            {
                await AppDbContext.Violations.AddAsync(
                    new Violation()
                    {
                        Id = Ulid.NewUlid().ToGuid(),
                        UserId = userId,
                        BookingId = booking.Id,
                        Type = ViolationTypeEnum.强制取消,
                        Content = "在预约开始前3个小时内强制取消预约违规",
                        CreateTime = DateTime.UtcNow,
                        State = ViolationStateEnum.Violation
                    }
                );
            }

        booking.State = BookingStateEnum.已取消;

        var track = AppDbContext.Bookings.Update(booking);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("取消预约失败");

        return track.Entity;
    }

    public async Task<Booking> CheckIn(Guid booingId, Guid userId)
    {
        var booking = await GetById(booingId);
        var user = await UserService.GetUserById(userId);
        if (user.Role is not UserRoleEnum.Admin && booking.UserId != user.Id)
            throw new ForbidException("没有权限");

        if (booking.State is not BookingStateEnum.已预约)
            throw new BadHttpRequestException("必须已预约的才能签到");

        if ((booking.StartTime - DateTime.UtcNow).Duration() > TimeSpan.FromMinutes(15))
            throw new BadHttpRequestException("距离开始时间超过15分钟，不可签到");


        booking.State = BookingStateEnum.已签到;
        booking.CheckInTime = DateTime.UtcNow;

        var track = AppDbContext.Bookings.Update(booking);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("签到失败");

        return track.Entity;
    }

    public async Task<Booking> CheckOut(Guid booingId, Guid userId)
    {
        var booking = await GetById(booingId);
        var user = await UserService.GetUserById(userId);
        if (user.Role is not UserRoleEnum.Admin && booking.UserId != user.Id)
            throw new ForbidException("没有权限");

        if (booking.State is not BookingStateEnum.已签到)
            throw new BadHttpRequestException("必须已预约的才能签到");

        if ((booking.EndTime - DateTime.UtcNow).Duration() > TimeSpan.FromMinutes(15))
            throw new BadHttpRequestException("距离结束时间超过15分钟，不可签退");


        booking.State = BookingStateEnum.已签退;
        booking.CheckOutTime = DateTime.UtcNow;

        await using var transaction = await AppDbContext.Database.BeginTransactionAsync();

        // 完成一次预约添加积分
        await UserService.UpdateUser(user with { Credits = Math.Min(user.Credits + 5, 100) });

        var track = AppDbContext.Bookings.Update(booking);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("签退失败");

        return track.Entity;
    }

    public async Task Delete(Guid bookingId)
    {
        var booking = await GetById(bookingId);
        AppDbContext.Bookings.Remove(booking);
        await AppDbContext.SaveChangesAsync();
        return;
    }
}