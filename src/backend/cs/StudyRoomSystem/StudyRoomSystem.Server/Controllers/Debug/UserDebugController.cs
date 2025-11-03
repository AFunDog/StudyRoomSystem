using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Server.Controllers.Filters;
using StudyRoomSystem.Server.Database;

namespace StudyRoomSystem.Server.Controllers.Debug;


#if DEBUG

[ApiController]
[Route("api/v{version:apiVersion}/user-debug")]
[DevOnly]
public class UserDebugController : ControllerBase
{
    private AppDbContext AppDbContext { get; }
    
    public UserDebugController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }
    
    [DevOnly]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await AppDbContext.Users.AsNoTracking().ToListAsync());
    }

    [DevOnly]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await AppDbContext.Users.SingleOrDefaultAsync(x => x.Id == id));
    }

    [DevOnly]
    [HttpPost]
    public async Task<IActionResult> Post(User user)
    {
        var track = AppDbContext.Users.Update(user);
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
        var res = await AppDbContext.Users.Where(x => x.Id == id).ExecuteDeleteAsync();
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