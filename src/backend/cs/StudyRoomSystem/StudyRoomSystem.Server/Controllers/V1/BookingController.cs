using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/booking")]
[ApiVersion("1.0")]
public class BookingController(AppDbContext appDbContext, IBookingService bookingService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IBookingService BookingService { get; } = bookingService;

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType<ApiPageResult<Booking>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("获取所有我的预约")]
    public async Task<IActionResult> GetMyBookings(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await BookingService.GetAllByUserId(this.GetLoginUserId(), page, pageSize));
    }

    [HttpGet("all")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<IEnumerable<Booking>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("管理员获取所有预约")]
    public async Task<IActionResult> GetAllBookings(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20,
        [FromQuery] Guid? roomId = null,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null,
        [FromQuery] BookingStateEnum? state = null)
    {
        return Ok(await BookingService.GetAll(page, pageSize, roomId, startTime, endTime, state));
    }


    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("获取指定预约信息")]
    public async Task<IActionResult> GetBooking(Guid id)
    {
        return Ok(await BookingService.GetById(id));
    }


    [HttpPost]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("创建预约")]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        return Ok(
            await BookingService.Create(
                new Booking
                {
                    UserId = this.GetLoginUserId(),
                    SeatId = request.SeatId,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    State = BookingStateEnum.已预约
                }
            )
        );
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("取消预约")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromQuery] bool isForce = false)
    {
        // var userId = this.GetLoginUserId();
        // var booking = await AppDbContext.Bookings.SingleOrDefaultAsync(b => b.Id == id);
        // if (booking is null)
        //     return NotFound(new ProblemDetails { Title = "预约不存在" });
        // if (booking.UserId != userId)
        //     return Forbid();
        //
        // if (booking.State != BookingStateEnum.已预约)
        //     return BadRequest(new ProblemDetails() { Title = "必须在预约中的才能取消预约" });
        //
        // booking.State = BookingStateEnum.已取消;
        //
        // // TODO 检查取消预约的时间，并检查可能违规行为
        // if (booking.StartTime - DateTime.UtcNow < TimeSpan.FromHours(3))
        //     if (!isForce)
        //         return BadRequest(new ProblemDetails() { Title = "距离预约起始时间小于3小时，若强制取消预约将会记录为违规" });
        //     else
        //     {
        //         await AppDbContext.Violations.AddAsync(
        //             new Violation()
        //             {
        //                 Id = Ulid.NewUlid().ToGuid(),
        //                 UserId = userId,
        //                 BookingId = booking.Id,
        //                 Type = ViolationTypeEnum.强制取消,
        //                 Content = "在预约开始前3个小时内强制取消预约违规",
        //                 CreateTime = DateTime.UtcNow,
        //                 State = ViolationStateEnum.Violation
        //             }
        //         );
        //     }
        //
        // var track = AppDbContext.Bookings.Update(booking);
        // await AppDbContext.SaveChangesAsync();
        // return Ok(track.Entity);

        return Ok(await BookingService.Cancel(id, this.GetLoginUserId(), isForce));
    }


    [HttpPost("check-in")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("签到")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request)
    {
        return Ok(await BookingService.CheckIn(request.Id, this.GetLoginUserId()));
    }

    [HttpPost("check-out")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Booking>(StatusCodes.Status200OK)]
    [EndpointSummary("签退")]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request)
    {
        return Ok(await BookingService.CheckOut(request.Id, this.GetLoginUserId()));
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await BookingService.Delete(id);
        return Ok();
    }
}