using System.ComponentModel.DataAnnotations;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using StudyRoomSystem.Server.Services;

namespace StudyRoomSystem.Server.Controllers.V1;

/// <summary>
/// 投诉控制器
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/complaint")]
[ApiVersion("1.0")]
public class ComplaintController(
    AppDbContext appDbContext,
    IUserService userService,
    IComplaintService complaintService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IUserService UserService { get; } = userService;
    private IComplaintService ComplaintService { get; } = complaintService;

    [HttpGet]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ApiPageResult<Complaint>>(StatusCodes.Status200OK)]
    [EndpointSummary("获取所有投诉")]
    public async Task<IActionResult> GetAll(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await ComplaintService.GetAll(page, pageSize));
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType<Complaint>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [EndpointSummary("获取指定投诉")]
    public async Task<IActionResult> Get(Guid id)
    {
        return Ok(await ComplaintService.GetById(id));
    }

    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ApiPageResult<Complaint>>(StatusCodes.Status200OK)]
    [EndpointSummary("获取我发起的投诉")]
    public async Task<IActionResult> GetMy(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await ComplaintService.GetAllBySendUserId(this.GetLoginUserId(), page, pageSize));
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<Complaint>(StatusCodes.Status200OK)]
    [EndpointSummary("创建投诉")]
    public async Task<IActionResult> Create([FromBody] CreateComplaintRequest request)
    {
        return Ok(
            await ComplaintService.Create(
                new()
                {
                    SendUserId = this.GetLoginUserId(),
                    SeatId = request.SeatId,
                    State = ComplaintStateEnum.已发起,
                    Type = request.Type,
                    SendContent = request.Content,
                    TargetTime = request.TargetTime
                }
            )
        );
    }

    [HttpPut]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<Complaint>(StatusCodes.Status200OK)]
    [EndpointSummary("修改投诉")]
    public async Task<IActionResult> Edit([FromBody] EditComplaintRequest request)
    {
        var complaint = await ComplaintService.GetById(request.Id);

        complaint.Type = request.Type ?? complaint.Type;
        complaint.SendContent = request.Content ?? complaint.SendContent;
        complaint.TargetTime = request.TargetTime ?? complaint.TargetTime;

        return Ok(await ComplaintService.Update(complaint));
    }

    [HttpPut("handle")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Complaint>(StatusCodes.Status200OK)]
    [EndpointSummary("处理投诉请求")]
    public async Task<IActionResult> Handle([FromBody] HandleComplaintRequest request)
    {
        return Ok(
            await ComplaintService.Handle(
                request.Id,
                this.GetLoginUserId(),
                request.TargetUserId,
                request.Content,
                request.ViolationContent,
                request.Score
            )
        );
    }

    [HttpPut("close")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<Complaint>(StatusCodes.Status200OK)]
    [EndpointSummary("关闭投诉请求")]
    public async Task<IActionResult> Close([FromBody] CloseComplaintRequest request)
    {
        // var userId = this.GetLoginUserId();
        // if (userId == Guid.Empty)
        //     return Unauthorized();
        //
        // var entity = await AppDbContext.Complaints.SingleOrDefaultAsync(x => x.Id == request.Id);
        // if (entity is null)
        //     return NotFound();
        // if (entity.State is not ComplaintStateEnum.已发起)
        //     return BadRequest(new ProblemDetails() { Title = "投诉状态不是已发起状态" });
        // entity.State = ComplaintStateEnum.已关闭;
        // entity.HandleTime = DateTime.UtcNow;
        // entity.HandleUserId = userId;
        // var track = AppDbContext.Complaints.Update(entity);
        // await AppDbContext.SaveChangesAsync();
        return Ok(await ComplaintService.Close(request.Id,this.GetLoginUserId(),request.Content));
    }

    [HttpDelete]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("删除指定投诉")]
    public async Task<IActionResult> Delete(Guid id)
    {
        // var complaint = await AppDbContext.Complaints.SingleOrDefaultAsync(x => x.Id == id);
        // if (complaint is null)
        //     return NotFound();
        // AppDbContext.Complaints.Remove(complaint);
        // await AppDbContext.SaveChangesAsync();
        await ComplaintService.Delete(id);
        return Ok();
    }
}