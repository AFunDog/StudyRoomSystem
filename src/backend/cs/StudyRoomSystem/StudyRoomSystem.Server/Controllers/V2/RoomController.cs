using System.ComponentModel;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api.V2;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;

namespace StudyRoomSystem.Server.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/room")]
[ApiVersion("2.0")]
public class RoomController(AppDbContext appDbContext, IRoomService roomService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IRoomService RoomService { get; } = roomService;


    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<GetRoomResponseOk>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [EndpointSummary("获取指定的房间信息")]
    [EndpointDescription(
        """
        在获取房间信息时会附带房间内的所有座位信息
        如果提供了查询时间段，那么会附带返回该时间段没有占用的座位
        """
    )]
    public async Task<IActionResult> Get(
        Guid id,
        [FromQuery] [Description("查询可用时间段的起点时间")] DateTime? start = null,
        [FromQuery] [Description("查询可用时间段的结束时间")] DateTime? end = null)
    {
        var room = await RoomService.GetRoomById(id);

        var response = new GetRoomResponseOk() { Room = room, };

        if (start is not null && end is not null)
        {
            if (start.Value.Kind != DateTimeKind.Utc || end.Value.Kind != DateTimeKind.Utc)
                return BadRequest(new ProblemDetails() { Title = "时间必须是Utc时间" });

            var openSeats = new List<Guid>();

            foreach (var seat in room.Seats)
            {
                var res = seat.Bookings.Any(x
                    => (x.State != BookingStateEnum.已取消) && (start.Value <= x.EndTime && x.StartTime <= end.Value)
                );
                if (res)
                    continue;
                openSeats.Add(seat.Id);
            }

            response.Seats = openSeats.ToArray();
        }

        // 清空返回座位的Bookings信息
        foreach (var seat in response.Room.Seats)
        {
            seat.Bookings = [];
        }

        return Ok(response);
    }
}