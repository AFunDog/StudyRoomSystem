using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
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

    // [HttpGet]
    // [AllowAnonymous]
    // public async Task<IActionResult> GetAll()
    // {
    //     return Ok(await AppDbContext.Seats.AsNoTracking().Include(x => x.Room).ToListAsync());
    // }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(Guid id)
    {
        var seat = await AppDbContext.Seats.AsNoTracking().Include(x => x.Room).SingleOrDefaultAsync(x => x.Id == id);
        if (seat is null)
            return NotFound();
        return Ok(seat);
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
}