using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

/// <summary>
/// 投诉控制器
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/complaint")]
[ApiVersion("1.0")]
public class ComplaintController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public ComplaintController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    [HttpGet]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("获取所有投诉")]
    public async Task<IActionResult> GetAll()
    {
        return Ok();
    }

    [HttpGet("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("获取指定投诉")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok();
    }

    [HttpPost]
    [Authorize]
    [EndpointSummary("创建投诉")]
    public async Task<IActionResult> Create()
    {
        return Ok();
    }

    [HttpPut]
    [Authorize]
    [EndpointSummary("修改投诉")]
    public async Task<IActionResult> Edit()
    {
        return Ok();
    }
}
