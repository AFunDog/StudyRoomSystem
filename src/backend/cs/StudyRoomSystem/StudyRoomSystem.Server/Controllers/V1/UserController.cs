using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/user")]
[ApiVersion("1.0")]
public class UserController : ControllerBase
{
    private AppDbContext AppDbContext { get; }
    
    public UserController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLoginUser()
    {
        var userId = this.GetLoginUserId();
        if (userId == Guid.Empty)
            return Unauthorized();
        
        return Ok(await AppDbContext.Users.SingleOrDefaultAsync(x => x.Id == userId));
    }
    
    
}
