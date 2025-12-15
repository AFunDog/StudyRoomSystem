using System.ComponentModel;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Helpers;
using StudyRoomSystem.Core.Structs.Api.V2;
using StudyRoomSystem.Server.Database;

namespace StudyRoomSystem.Server.Controllers.V2;


[ApiController]
[Route("api/v{version:apiVersion}/seat")]
[ApiVersion("2.0")]
public class SeatController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public SeatController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }
    
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
        }
        return Ok(response);
    }
}
