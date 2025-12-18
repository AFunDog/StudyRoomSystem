using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace StudyRoomSystem.Server.Controllers.Debug;


[ApiController]
[Route("api/v{version:apiVersion}/test")]
[ApiVersion("1.0")]
public class TestController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Test()
    {
        return Forbid();
    }
}
