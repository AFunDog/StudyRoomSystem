using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Server.Database;

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

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await AppDbContext.Seats.AsNoTracking().ToListAsync());
    }
}
