using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

/// <summary>
/// 违规控制器
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/violation")]
[ApiVersion("1.0")]
public class ViolationController(AppDbContext appDbContext) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;


    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiPageResult<Violation>>(StatusCodes.Status200OK)]
    [EndpointSummary("获取所有我的违规记录")]
    public async Task<IActionResult> GetMyViolations(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();

        var query = AppDbContext
            .Violations
            .Where(x => x.UserId == userId)
            .Include(x => x.User)
            .Include(x => x.Booking)
            .AsNoTracking();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(
            new ApiPageResult<Violation>()
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Items = items
            }
        );
    }
    
    [HttpGet("all")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("管理员查看所有的违规记录")]
    public async Task<IActionResult> GetAllViolations(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        var query = AppDbContext
            .Violations.Include(x => x.User)
            .AsNoTracking();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(
            new ApiPageResult<Violation>()
            {
                Total = total,
                Page = page,
                PageSize = pageSize,
                Items = items
            }
        );
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Violation>(StatusCodes.Status200OK)]
    [EndpointSummary("查看指定违规记录")]
    public async Task<IActionResult> GetViolation(Guid id)
    {
        var violation = await AppDbContext.Violations
            .Include(x => x.User)
            .Include(x => x.Booking)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (violation == null)
            return NotFound("违规记录不存在");
        return Ok(violation);
    }

    [HttpPost]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<Violation>(StatusCodes.Status200OK)]
    [EndpointSummary("创建违规记录")]
    public async Task<IActionResult> Create([FromBody] CreateViolationRequest request)
    {
        var userId = Guid.TryParse(User.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        if (userId == Guid.Empty)
            return Unauthorized();
        var user = await AppDbContext.Users.FindAsync(request.UserId);
        if (user == null)
            return NotFound("违规用户不存在");
        var booking = await AppDbContext.Bookings.FindAsync(request.BookingId);
        if (booking == null)
            return NotFound("预约记录不存在");
        
        
        var track = await AppDbContext.Violations.AddAsync(
            new Violation
            {
                Id = Ulid.NewUlid().ToGuid(),
                UserId = request.UserId,
                BookingId = request.BookingId,
                CreateTime = DateTime.UtcNow,
                State = ViolationStateEnum.Violation,
                Type = request.Type,
                Content = request.Content
            }
        );
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("删除违规记录")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var violation = await AppDbContext.Violations.FindAsync(id);
        if (violation == null)
            return NotFound("违规记录不存在");
        AppDbContext.Violations.Remove(violation);
        await AppDbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Violation>(StatusCodes.Status200OK)]
    [EndpointSummary("修改违规记录")]
    public async Task<IActionResult> Edit([FromBody]EditViolationRequest request)
    {
        var violation = await AppDbContext.Violations.FindAsync(request.Id);
        if (violation == null)
            return NotFound("违规记录不存在");
        violation.Type= request.Type ?? violation.Type;
        violation.Content= request.Content ?? violation.Content;
        var track = AppDbContext.Violations.Update(violation);
        await AppDbContext.SaveChangesAsync();
        return Ok(track.Entity);
    }
}