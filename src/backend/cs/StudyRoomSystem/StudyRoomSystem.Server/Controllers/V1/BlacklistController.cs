using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using Microsoft.AspNetCore.Http;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Server.Contacts;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/blacklist")]
[ApiVersion("1.0")]
public class BlacklistController(AppDbContext appDbContext, IBlacklistService blacklistService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IBlacklistService BlacklistService { get; } = blacklistService;


    [HttpGet]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ApiPageResult<Blacklist>>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员获取所有黑名单")]
    public async Task<IActionResult> GetAllBlacklists(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await BlacklistService.GetAll(page, pageSize));
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [EndpointSummary("获取指定黑名单信息")]
    public async Task<IActionResult> GetBlacklist(Guid id)
    {
        return Ok(await BlacklistService.GetById(id));
    }

    [HttpPost]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员创建黑名单")]
    public async Task<IActionResult> Create([FromBody] CreateBlacklistRequest request)
    {
        return Ok(
            await BlacklistService.Create(
                new Blacklist()
                {
                    ExpireTime = request.ExpireTime,
                    UserId = request.UserId,
                    Reason = request.Reason,
                    Type = request.Type
                }
            )
        );
    }

    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [EndpointSummary("管理员更新黑名单")]
    public async Task<IActionResult> Edit([FromBody] EditBlacklistRequest request)
    {
        var entity = await BlacklistService.GetById(request.Id);

        entity.ExpireTime = request.ExpireTime ?? entity.ExpireTime;
        entity.Reason = request.Reason ?? entity.Reason;
        entity.Type = request.Type ?? entity.Type;
        
        return Ok(await BlacklistService.Update(entity));
    }

    [HttpDelete]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员删除黑名单")]
    public async Task<IActionResult> Delete(Guid id)
    {
         await BlacklistService.Delete(id);
         return Ok();
    }
}