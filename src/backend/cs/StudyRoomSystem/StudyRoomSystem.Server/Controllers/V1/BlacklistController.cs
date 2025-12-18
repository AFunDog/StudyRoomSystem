using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using Microsoft.AspNetCore.Http;
using StudyRoomSystem.Core.Structs.Api.V1;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/blacklist")]
[ApiVersion("1.0")]
public class BlacklistController(AppDbContext appDbContext) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;


    [HttpGet]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Blacklist[]>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员获取所有黑名单")]
    public async Task<IActionResult> GetAllBlacklists(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        var blacklists = await AppDbContext.Blacklists.AsNoTracking().Page(page, pageSize).ToListAsync();
        return Ok(blacklists);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [EndpointSummary("获取指定黑名单信息")]
    public async Task<IActionResult> GetBlacklist(Guid id)
    {
        var blacklist = await AppDbContext.Blacklists.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (blacklist == null)
        {
            return NotFound();
        }

        return Ok(blacklist);
    }

    [HttpPost]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员创建黑名单")]
    public async Task<IActionResult> Create([FromBody] CreateBlacklistRequest request)
    {
        var track = await AppDbContext.Blacklists.AddAsync(
            new Blacklist()
            {
                Id = Ulid.NewUlid().ToGuid(),
                CreateTime = DateTime.UtcNow,
                ExpireTime = request.ExpireTime,
                UserId = request.UserId,
                Reason = request.Reason,
                Type = request.Type
            }
        );
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }

    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [EndpointSummary("管理员更新黑名单")]
    public async Task<IActionResult> Edit([FromBody] EditBlacklistRequest request)
    {
        var entity = await AppDbContext.Blacklists.SingleOrDefaultAsync(x => x.Id == request.Id);
        if (entity == null)
        {
            return NotFound();
        }

        entity.ExpireTime = request.ExpireTime ?? entity.ExpireTime;
        entity.Reason = request.Reason ?? entity.Reason;
        entity.Type = request.Type ?? entity.Type;
        var track = AppDbContext.Blacklists.Update(entity);
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }
    
    [HttpDelete]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Blacklist>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员删除黑名单")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await AppDbContext.Blacklists.SingleOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }
        AppDbContext.Blacklists.Remove(entity);
        await AppDbContext.SaveChangesAsync();
        return Ok();
    }
}