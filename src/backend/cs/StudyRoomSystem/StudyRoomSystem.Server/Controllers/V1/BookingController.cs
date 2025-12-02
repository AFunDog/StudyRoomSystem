using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/booking")]
[ApiVersion("1.0")]
public class BookingController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public BookingController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType<IEnumerable<Booking>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("获取所有我的预约")]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        return Ok(
            await AppDbContext
                .Bookings.Include(b => b.Seat)
                .Include(b => b.Seat.Room)
                .Where(x => x.UserId == userId)
                .ToListAsync()
        );
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("获取指定预约信息")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var booking = await AppDbContext.Bookings.FirstOrDefaultAsync(x => x.Id == id);
        if (booking is null)
            return NotFound();

        return Ok(booking);
    }


    [HttpPost]
    [Authorize]
    [ProducesResponseType<ResponseError>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("创建预约")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var seat = await AppDbContext.Seats.FirstOrDefaultAsync(x => x.Id == request.SeatId);
        if (seat is null)
            return NotFound(new ResponseError() { Message = "找不到座位" });

        if (request.StartTime >= request.EndTime)
            return BadRequest(new ResponseError() { Message = "开始时间不能大于结束时间" });

        // 检查座位是否在时间段内被占用
        var booking = await AppDbContext.Bookings.FirstOrDefaultAsync(x
            => x.SeatId == request.SeatId
               && ((x.StartTime <= request.StartTime && request.StartTime <= x.EndTime)
                   || (x.StartTime <= request.EndTime && request.EndTime <= x.EndTime))
               && (x.State == (BookingStateEnum.Booking) || x.State == (BookingStateEnum.CheckIn))
        );
        if (booking is not null)
            return Conflict(new { message = "座位在时间范围内已被用户预约" });

        var track = await AppDbContext.Bookings.AddAsync(
            new Booking
            {
                Id = Ulid.NewUlid().ToGuid(),
                UserId = userId,
                SeatId = seat.Id,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreateTime = DateTime.UtcNow,
                State = BookingStateEnum.Booking
            }
        );
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ResponseError>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("取消预约")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromQuery] bool isForce = false)
    {
        var userId = this.GetLoginUserId();
        var booking = await AppDbContext.Bookings.SingleOrDefaultAsync(b => b.Id == id);
        if (booking is null)
            return NotFound(new ResponseError { Message = "预约不存在" });
        if (booking.UserId != userId)
            return Forbid();

        if (booking.State != BookingStateEnum.Booking)
            return BadRequest(new ResponseError() { Message = "必须在预约中的才能取消预约" });

        booking.State = BookingStateEnum.Canceled;

        // TODO 检查取消预约的时间，并检查可能违规行为
        if (booking.StartTime - DateTime.UtcNow < TimeSpan.FromHours(3))
            if (!isForce)
                return BadRequest(new ResponseError() { Message = "距离预约起始时间小于3小时，若强制取消预约将会记录为违规" });


        AppDbContext.Bookings.Update(booking);
        await AppDbContext.SaveChangesAsync();
        return Ok();
    }



    [HttpPut]
    [Authorize]
    [ProducesResponseType<ResponseError>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("修改预约")]
    public async Task<IActionResult> EditBooking([FromBody] EditBookingRequest request)
    {
        var userId = this.GetLoginUserId();
        var booking = await AppDbContext.Bookings.SingleOrDefaultAsync(b => b.Id == request.Id);
        if (booking is null)
            return NotFound(new ResponseError() { Message = "预约不存在" });
        if (booking.UserId != userId)
            return Forbid();
        if (request.StartTime >= request.EndTime)
            return BadRequest(new ResponseError() { Message = "开始时间不能大于结束时间" });
        if (booking.State != BookingStateEnum.Booking)
            return BadRequest(new ResponseError() { Message = "必须在预约中的才能修改预约" });

        // 修改后的起始时间距离预约小于3小时
        if (booking.StartTime - DateTime.UtcNow < TimeSpan.FromHours(3))
            return BadRequest(new ResponseError() { Message = "距离预约起始时间小于3小时，不可修改" });

        booking.StartTime = request.StartTime ?? booking.StartTime;
        booking.EndTime = request.EndTime ?? booking.EndTime;

        // 检查座位是否在时间段内被占用
        var existBooking = await AppDbContext.Bookings.FirstOrDefaultAsync(x
            => x.SeatId == booking.SeatId
               && ((x.StartTime <= request.StartTime && request.StartTime <= x.EndTime)
                   || (x.StartTime <= request.EndTime && request.EndTime <= x.EndTime))
               && (x.State == (BookingStateEnum.Booking) || x.State == (BookingStateEnum.CheckIn))
        );
        if (existBooking is not null)
            return Conflict(new ResponseError(){ Message = "座位在时间范围内已被用户预约" });


        var track = AppDbContext.Bookings.Update(booking);
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }



    [HttpPost("check-in")]
    [Authorize]
    [ProducesResponseType<ResponseError>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("签到")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        var userId = this.GetLoginUserId();
        var booking = await AppDbContext.Bookings.SingleOrDefaultAsync(b => b.Id == request.Id);
        if (booking is null)
            return NotFound(new ResponseError(){ Message = "预约不存在" });
        if (booking.UserId != userId)
            return Forbid();

        if (booking.State is not (BookingStateEnum.Booking))
            return BadRequest(new ResponseError(){ Message = "当前状态不能签到" });

        // TODO 由于定时系统自动清理逾期预约，所以可以不检查预约时间
        if ((booking.StartTime - DateTime.UtcNow).Duration() > TimeSpan.FromMinutes(15))
            return BadRequest(new ResponseError(){ Message = "距离开始时间超过15分钟，不可签到" });

        booking.State = (BookingStateEnum.CheckIn);
        var track = AppDbContext.Bookings.Update(booking);
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }

    [HttpPost("check-out")]
    [Authorize]
    [ProducesResponseType<ResponseError>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("签退")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
        var userId = this.GetLoginUserId();
        var booking = await AppDbContext.Bookings.SingleOrDefaultAsync(b => b.Id == request.Id);
        if (booking is null)
            return NotFound(new ResponseError(){ Message = "预约不存在" });
        if (booking.UserId != userId)
            return Forbid();
        if (booking.State is not BookingStateEnum.CheckIn)
            return BadRequest(new ResponseError(){ Message = "当前状态不能签退" });

        // WHY 能不能提前签退
        if ((booking.EndTime - DateTime.UtcNow).Duration() > TimeSpan.FromMinutes(15))
            return BadRequest(new ResponseError(){ Message = "距离结束时间超过15分钟，不可签退" });

        booking.State = (BookingStateEnum.Checkout);
        var track = AppDbContext.Bookings.Update(booking);
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }
}