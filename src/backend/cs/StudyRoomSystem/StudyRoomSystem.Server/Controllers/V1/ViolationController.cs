using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

/// <summary>
/// 违规控制器
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/violation")]
[ApiVersion("1.0")]
public class ViolationController(AppDbContext appDbContext, IViolationService violationService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IViolationService ViolationService { get; } = violationService;


    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiPageResult<Violation>>(StatusCodes.Status200OK)]
    [EndpointSummary("获取所有我的违规记录")]
    public async Task<IActionResult> GetMyViolations(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await ViolationService.GetAllByUserId(this.GetLoginUserId(), page, pageSize));
    }

    [HttpGet("all")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("管理员查看所有的违规记录")]
    public async Task<IActionResult> GetAllViolations(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await ViolationService.GetAll(page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Violation>(StatusCodes.Status200OK)]
    [EndpointSummary("查看指定违规记录")]
    public async Task<IActionResult> GetViolation(Guid id)
    {
        // var violation = await AppDbContext
        //     .Violations.Include(x => x.User)
        //     .Include(x => x.Booking)
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(x => x.Id == id);
        // if (violation == null)
        //     return NotFound("违规记录不存在");
        // return Ok(violation);

        return Ok(await ViolationService.GetById(id));
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
        return Ok(
            await ViolationService.Create(
                new Violation
                {
                    UserId = request.UserId,
                    BookingId = request.BookingId,
                    State = ViolationStateEnum.Violation,
                    Type = request.Type,
                    Content = request.Content
                }
            )
        );
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("删除违规记录")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await ViolationService.Delete(id);
        return Ok();
    }

    [HttpPut]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Violation>(StatusCodes.Status200OK)]
    [EndpointSummary("修改违规记录")]
    public async Task<IActionResult> Edit([FromBody] EditViolationRequest request)
    {
        var violation = await ViolationService.GetById(request.Id);
        violation.Type = request.Type ?? violation.Type;
        violation.Content = request.Content ?? violation.Content;
        return Ok(await ViolationService.Update(violation));
    }
}