using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
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
    public async Task<IActionResult> GetAll()
    {
        return Ok(await AppDbContext.Rooms.Include(r => r.Seats).AsNoTracking().ToListAsync());
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id)
    {
        var room = await AppDbContext.Rooms.Include(r => r.Seats).AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (room is null)
            return NotFound();
        return Ok(room);
    }

    public class CreateRoomRequest
    {
        public required string Name { get; set; }
        public required TimeOnly OpenTime { get; set; }
        public required TimeOnly CloseTime { get; set; }

        [Range(1, 2048)]
        public required int Rows { get; set; }
        [Range(1, 2048)]
        public required int Cols { get; set; }
    }

    [HttpPost]
    [Authorize(AuthorizationHelper.Policy.Admin)]
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

        return BadRequest(new { message = "数据添加失败" });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    public async Task<IActionResult> DeleteRoom(Guid id)
    {
        var room = await AppDbContext.Rooms.SingleOrDefaultAsync(x => x.Id == id);
        if (room is null)
            return NotFound();
        
        AppDbContext.Rooms.Remove(room);
        var res = await AppDbContext.SaveChangesAsync();
        return res != 0 ? Ok() : BadRequest(new { message = "数据删除失败" });
    }
}