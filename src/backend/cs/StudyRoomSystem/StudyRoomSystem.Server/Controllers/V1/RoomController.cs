using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/room")]
[ApiVersion("1.0")]
public class RoomController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public RoomController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<Room[]>(StatusCodes.Status200OK)]
    [EndpointSummary("获取所有的房间信息")]
    [EndpointDescription("在获取房间信息时会附带房间内的所有座位信息")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await AppDbContext.Rooms.Include(r => r.Seats).AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<Room>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("获取指定的房间信息")]
    [EndpointDescription("在获取房间信息时会附带房间内的所有座位信息")]
    public async Task<IActionResult> Get(Guid id)
    {
        var room = await AppDbContext.Rooms.Include(r => r.Seats).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (room is null)
            return NotFound();
        return Ok(room);
    }



    /// <summary>
    /// 管理员创建房间
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Room>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [EndpointSummary("管理员创建房间")]
    // [EndpointDescription("在获取房间信息时会附带房间内的所有座位信息")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        var room = new Room()
        {
            Id = Ulid.NewUlid().ToGuid(),
            Name = request.Name,
            OpenTime = request.OpenTime,
            CloseTime = request.CloseTime,
            Rows = request.Rows,
            Cols = request.Cols
        };
        var track = await AppDbContext.Rooms.AddAsync(room);
        var res = await AppDbContext.SaveChangesAsync();
        if (res != 0)
        {
            return Ok(track.Entity);
        }

        return Conflict(new ProblemDetails(){ Title = "数据添加失败" });
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [EndpointSummary("管理员创建房间")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        var room = await AppDbContext.Rooms.SingleOrDefaultAsync(x => x.Id == id);
        if (room is null)
            return NotFound(new ProblemDetails(){ Title = "找不到房间" });
        
        AppDbContext.Rooms.Remove(room);
        var res = await AppDbContext.SaveChangesAsync();
        return res != 0 ? Ok() : Conflict(new ProblemDetails(){ Title = "数据删除失败" });
    }
    
    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Room>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [EndpointSummary("管理员修改房间信息")]
    public async Task<IActionResult> EditRoom([FromBody] EditRoomRequest request)
    {
        var room = await AppDbContext.Rooms.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (room is null)
            return NotFound(new ProblemDetails(){ Title = "管理员找不到房间" });
        
        room.Name = request.Name ?? room.Name;
        room.OpenTime = request.OpenTime ?? room.OpenTime;
        room.CloseTime = request.CloseTime ?? room.CloseTime;
        room.Rows = request.Rows ?? room.Rows;
        room.Cols = request.Cols ?? room.Cols;
        
        // TODO 数据校验

        var track = AppDbContext.Rooms.Update(room);
        await AppDbContext.SaveChangesAsync();
        
        return Ok(track.Entity);
    }
}