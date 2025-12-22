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
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/seat")]
[ApiVersion("1.0")]
public class SeatController(AppDbContext appDbContext, IRoomService roomService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IRoomService RoomService { get; } = roomService;

    [ApiVersion(1.0)]
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Seat>(StatusCodes.Status200OK)]
    [EndpointSummary("获取指定座位信息")]
    [EndpointDescription("获取座位信息时会附带所在房间信息")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await RoomService.GetSeatById(id));
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
        return Ok(
            await RoomService.AddSeat(
                new Seat()
                {
                    Col = request.Col,
                    Row = request.Row,
                    RoomId = request.RoomId
                }
            )
        );
        // return CreatedAtAction(nameof(Get), new { id = newSeat.Id }, newSeat);
    }

    // [HttpPut]
    // [Authorize(AuthorizationHelper.Policy.Admin)]
    // [EndpointSummary("管理员修改座位信息")]
    // public async Task<IActionResult> Edit()
    // {
    //     return Ok();
    // }


    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("管理员删除座位")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await RoomService.DeleteSeat(id);
        return Ok();
    }
}