using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Server.Controllers.Filters;
using StudyRoomSystem.Server.Database;

namespace StudyRoomSystem.Server.Controllers.Debug;


#if DEBUG_

[ApiController]
[Route("api/v{version:apiVersion}/seat-debug")]
[DevOnly]
public class SeatDebugController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public SeatDebugController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    [DevOnly]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await AppDbContext.Seats
            .Include(s => s.Room)
            .AsNoTracking().ToListAsync());
    }

    [DevOnly]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await AppDbContext.Seats
            .Include(s => s.Room)
            .SingleOrDefaultAsync(x => x.Id == id));
    }

    [DevOnly]
    [HttpPost]
    public async Task<IActionResult> Post(Seat data)
    {
        var track = AppDbContext.Seats.Update(data);
        var res = await AppDbContext.SaveChangesAsync();
        if (res != 0)
        {
            return Ok(track.Entity);
        }

        return Conflict(
            new
            {
                message = "更新失败"
            }
        );
    }

    [DevOnly]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var res = await AppDbContext.Seats.Where(x => x.Id == id).ExecuteDeleteAsync();
        if (res != 0)
        {
            return Ok(
                new
                {
                    message = "删除成功"
                }
            );
        }
        else
        {
            return Conflict(
                new
                {
                    message = "删除失败"
                }
            );
        }
    }
}

#endif