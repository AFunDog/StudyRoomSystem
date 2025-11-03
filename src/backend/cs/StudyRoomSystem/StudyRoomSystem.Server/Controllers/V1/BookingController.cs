using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
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
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        return Ok(await AppDbContext.Bookings.Include(b => b.Seat).Include(b => b.Seat.Room).Where(x => x.UserId == userId).ToListAsync());
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        var booking = await AppDbContext.Bookings.FirstOrDefaultAsync(x => x.Id == id);
        if (booking is null)
            return NotFound();

        return Ok(booking);
    }

    public class CreateBookingRequest
    {
        public required Guid SeatId { get; set; }
        public required DateTime StartTime { get; set; }
        public required DateTime EndTime { get; set; }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        // 检查座位是否在时间段内被占用
        var seat = await AppDbContext.Seats.FirstOrDefaultAsync(x => x.Id == request.SeatId);
        var booking = await AppDbContext.Bookings.FirstOrDefaultAsync(x
            => x.SeatId == request.SeatId
               && ((x.StartTime <= request.StartTime && request.StartTime <= x.EndTime)
                   || (x.StartTime <= request.EndTime && request.EndTime <= x.EndTime))
        );
        if (seat is null)
            return NotFound(new { message = "找不到座位" });
        if (booking is not null)
            return Conflict(new { message = "座位在时间范围内已被用户预约" });

        var userId = Guid.TryParse(User.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var track = await AppDbContext.Bookings.AddAsync(
            new Booking
            {
                Id = Ulid.NewUlid().ToGuid(),
                UserId = userId,
                SeatId = seat.Id,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreateTime = DateTime.UtcNow
            }
        );
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteBooking(Guid id)
    {
        var userId = this.GetLoginUserId();
        var booking = await AppDbContext.Bookings.SingleOrDefaultAsync(b => b.Id == id);
        if (booking is null)
            return NotFound(new { message = "预约不存在" });
        if (booking.UserId != userId)
            return Forbid();

        // TODO 检查取消预约的时间，并检查可能违规行为
        if (booking.StartTime - DateTime.UtcNow < TimeSpan.FromHours(3))
            return BadRequest(new { message = "距离预约起始时间小于3小时，若强制取消预约将会记录为违规" });

        if (booking.CheckInTime is not null)
            return BadRequest(new { message = "已签到，不能取消预约" });

        AppDbContext.Bookings.Remove(booking);
        await AppDbContext.SaveChangesAsync();
        return Ok();
    }
}