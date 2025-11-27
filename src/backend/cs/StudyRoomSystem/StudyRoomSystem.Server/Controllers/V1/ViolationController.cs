using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

/// <summary>
/// 违规控制器
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/violation")]
[ApiVersion("1.0")]
public class ViolationController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public ViolationController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    
    [HttpGet]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("查看所有的违规记录")]
    public async Task<IActionResult> GetAll()
    {
        return Ok();
    }

    [HttpGet("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("查看指定违规记录")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok();
    }

    [HttpPost]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("创建违规记录")]
    public async Task<IActionResult> Create()
    {
        return Ok();
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("删除违规记录")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return Ok();
    }

    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("修改违规记录")]
    public async Task<IActionResult> Edit()
    {
        return Ok();
    }
}
