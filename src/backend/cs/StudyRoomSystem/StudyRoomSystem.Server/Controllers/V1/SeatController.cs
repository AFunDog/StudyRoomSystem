using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Helpers;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/seat")]
[ApiVersion("1.0")]
public class SeatController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public SeatController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    [ApiVersion(1.0)]
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Seat>(StatusCodes.Status200OK)]
    [EndpointSummary("获取指定座位信息")]
    [EndpointDescription("获取座位信息时会附带所在房间信息")]
    public async Task<IActionResult> Get(Guid id)
    {
        var seat = await AppDbContext.Seats.AsNoTracking().Include(x => x.Room).SingleOrDefaultAsync(x => x.Id == id);
        if (seat is null)
            return NotFound();
        return Ok(seat);
    }
    
    [ApiVersion(2.0)]
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<GetSeatResponseOk>(StatusCodes.Status200OK)]
    [EndpointSummary("获取指定座位信息")]
    [EndpointDescription(
        """
        获取座位信息时会附带所在房间信息和座位的占用时间信息
        """
    )]
    public async Task<IActionResult> Get(
        Guid id,
        [FromQuery] [Description("查询可用时间段的起点时间")] DateTime? start = null,
        [FromQuery] [Description("查询可用时间段的结束时间")]DateTime? end = null)
    {
        var seat = await AppDbContext
            .Seats.AsNoTracking()
            .Include(x => x.Room)
            // .Include(x => x.Bookings)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (seat is null)
            return NotFound();


        var response = new GetSeatResponseOk()
        {
            Seat = seat,
        };

        if (start is not null && end is not null)
        {
            if (start.Value.Kind != DateTimeKind.Utc || end.Value.Kind != DateTimeKind.Utc)
                return BadRequest(new ProblemDetails() { Title = "时间必须是Utc时间" });

            response.OpenTimes = (await AppDbContext
                    .Bookings.AsNoTracking()
                    .Where(x => x.SeatId == id)
                    .Where(x => (start.Value <= x.EndTime && x.StartTime <= end.Value))
                    .Select(x => new KeyValuePair<DateTime, DateTime>(x.StartTime, x.EndTime))
                    .ToListAsync())
                .ToOpenTimes(start.Value, end.Value)
                .ToArray();


            // var openTimes = new List<KeyValuePair<TimeOnly, TimeOnly>>();
            //
            // var startTime = seat.Room.OpenTime;
            // foreach (var (start,end) in fillTimes)
            // {
            //     if (startTime < start.ToTimeOnly())
            //     {
            //         openTimes.Add(new(startTime, start.ToTimeOnly()));
            //         startTime = end.ToTimeOnly();
            //     }
            // }
            //
            // if (startTime < seat.Room.CloseTime)
            // {
            //     openTimes.Add(new(startTime, seat.Room.CloseTime));
            // }
        }


        return Ok(response);
    }

    public class CreateSeatRequest
    {
        public required Guid RoomId { get; set; }
        public required int Row { get; set; }
        public required int Col { get; set; }
    }

    /// <summary>
    /// 管理员创建座位
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("管理员创建座位")]
    public async Task<IActionResult> Create(CreateSeatRequest request)
    {
        // 检查房间是否存在
        var room = await AppDbContext.Rooms.Include(x => x.Seats).SingleOrDefaultAsync(x => x.Id == request.RoomId);
        if (room is null)
            return NotFound(new { message = "房间不存在" });

        // 检查是否已经在该位置存在座位了
        var seat = room.Seats.FirstOrDefault(x
            => x.RoomId == request.RoomId && x.Row == request.Row && x.Col == request.Col
        );
        if (seat is not null)
            return Conflict(new { message = "该位置座位已存在" });

        var newSeat = new Seat()
            { RoomId = request.RoomId, Id = Ulid.NewUlid().ToGuid(), Row = request.Row, Col = request.Col };
        await AppDbContext.Seats.AddAsync(newSeat);
        await AppDbContext.SaveChangesAsync();
        return Ok(newSeat);
        // return CreatedAtAction(nameof(Get), new { id = newSeat.Id }, newSeat);
    }

    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("管理员修改座位信息")]
    public async Task<IActionResult> Edit()
    {
        return Ok();
    }


    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("管理员删除座位")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var seat = await AppDbContext.Seats.Include(x => x.Bookings).SingleOrDefaultAsync(x => x.Id == id);
        if (seat is null)
            return NotFound(new { message = "座位不存在" });

        // 检查是否有未结束的预约
        var hasActiveBooking = seat.Bookings.Any(booking => booking.EndTime > DateTime.UtcNow);
        if (hasActiveBooking)
            return BadRequest(new { message = "该座位有未结束的预约，无法删除" });

        AppDbContext.Seats.Remove(seat);
        await AppDbContext.SaveChangesAsync();
        return Ok();
    }
}