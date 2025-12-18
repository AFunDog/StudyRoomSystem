using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Server.Database;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/blacklist")]
[ApiVersion("1.0")]
public class BlacklistController(AppDbContext appDbContext) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    
    
    
    
}
