using System;
using System.ComponentModel.DataAnnotations;
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
[Route("api/v{version:apiVersion}/room")]
[ApiVersion("1.0")]
public class RoomController(AppDbContext appDbContext, IRoomService roomService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IRoomService RoomService { get; } = roomService;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<ApiPageResult<Room>>(StatusCodes.Status200OK)]
    [EndpointSummary("获取所有的房间信息")]
    [EndpointDescription("在获取房间信息时会附带房间内的所有座位信息")]
    public async Task<IActionResult> GetAll(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await RoomService.GetAllRoom(page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<Room>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [EndpointSummary("获取指定的房间信息")]
    [EndpointDescription("在获取房间信息时会附带房间内的所有座位信息")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await RoomService.GetRoomById(id));
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
        return Ok(
            await RoomService.CreateRoom(
                new Room()
                {
                    Name = request.Name,
                    OpenTime = request.OpenTime,
                    CloseTime = request.CloseTime,
                    Rows = request.Rows,
                    Cols = request.Cols
                }
            )
        );
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [EndpointSummary("管理员删除房间")]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        return Ok(await RoomService.DeleteRoom(id));
    }

    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Room>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [EndpointSummary("管理员修改房间信息")]
    public async Task<IActionResult> EditRoom([FromBody] EditRoomRequest request)
    {
        var room = await RoomService.GetRoomById(request.Id);

        room.Name = request.Name ?? room.Name;
        room.OpenTime = request.OpenTime ?? room.OpenTime;
        room.CloseTime = request.CloseTime ?? room.CloseTime;
        room.Rows = request.Rows ?? room.Rows;
        room.Cols = request.Cols ?? room.Cols;

        return Ok(await RoomService.UpdateRoom(room));
    }
}