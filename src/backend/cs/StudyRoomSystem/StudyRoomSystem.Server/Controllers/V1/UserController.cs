using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/user")]
[ApiVersion("1.0")]
public class UserController : ControllerBase
{
    private AppDbContext AppDbContext { get; }

    public UserController(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }

    [HttpGet]
    [Authorize]
    [EndpointSummary("获取登录用户的基本信息")]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoginUser()
    {
        var userId = this.GetLoginUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        return Ok(await AppDbContext.Users.SingleOrDefaultAsync(x => x.Id == userId));
    }


    #region Register

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [EndpointSummary("用户注册")]
    [EndpointDescription("用户需要使用该接口注册，注册成功之后需要使用用户名密码登录")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        // 检查用户名是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName)) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "用户名已存在"
                }
            );
        }

        // 检查学号/工号是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.CampusId == request.CampusId)) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "学号/工号已存在"
                }
            );
        }

        // 检查手机号是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.Phone == request.Phone)) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "手机号已存在"
                }
            );
        }

        // 检查邮箱是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x =>
                !string.IsNullOrEmpty(x.Email) && x.Email == request.Email
            )) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "邮箱已存在"
                }
            );
        }

        var addUser = await AppDbContext.Users.AddAsync(CreateUser(request, "User"));

        var res = await AppDbContext.SaveChangesAsync();
        if (res != 0)
        {
            return Ok(addUser.Entity);
        }
        else
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "用户注册失败"
                }
            );
        }
    }

    private static User CreateUser(RegisterRequest request, string role) => new()
    {
        Id = Ulid.NewUlid().ToGuid(),
        UserName = request.UserName,
        Password = PasswordHelper.HashPassword(request.Password),
        // 如果昵称为空，则默认使用用户名
        DisplayName = request.DisplayName ?? request.UserName,
        CampusId = request.CampusId,
        Phone = request.Phone,
        Email = request.Email,
        Role = role
    };

    [HttpPost("registerAdmin")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [EndpointSummary("管理员注册")]
    [EndpointDescription("管理员需要使用该接口注册，注册成功之后需要使用用户名密码登录")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        // 检查用户名是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName)) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "用户名已存在"
                }
            );
        }

        // 检查学号/工号是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.CampusId == request.CampusId)) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "学号/工号已存在"
                }
            );
        }

        // 检查手机号是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.Phone == request.Phone)) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "手机号已存在"
                }
            );
        }

        // 检查邮箱是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x =>
                !string.IsNullOrEmpty(x.Email) && x.Email == request.Email
            )) is not null)
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "邮箱已存在"
                }
            );
        }

        var addUser = await AppDbContext.Users.AddAsync(CreateUser(request, "Admin"));

        var res = await AppDbContext.SaveChangesAsync();
        if (res != 0)
        {
            return Ok(addUser.Entity);
        }
        else
        {
            return Conflict(
                new ProblemDetails()
                {
                    Title = "用户注册失败"
                }
            );
        }
    }

    #endregion


    #region Edit

    public class EditRequestNormal
    {
        public required Guid Id { get; set; }
        [MaxLength(128)] public required string DisplayName { get; set; }
        [MaxLength(64)] public required string CampusId { get; set; }
        [MaxLength(64)] [Phone] public required string Phone { get; set; }
        [MaxLength(64)] [EmailAddress] public string? Email { get; set; }
    }

    // TODO
    [HttpPut("information")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [EndpointSummary("用户更新基本信息")]
    public async Task<IActionResult> EditUserNormal([FromBody] EditRequestNormal request)
    {
        var userId = this.GetLoginUserId();
        var loginUser = await AppDbContext.Users.SingleOrDefaultAsync(b => b.Id == request.Id);
        if (loginUser is null)
            return NotFound(new ProblemDetails() { Title = "用户不存在" });
        loginUser.DisplayName = request.DisplayName;
        loginUser.CampusId = request.CampusId;
        loginUser.Phone = request.Phone;
        loginUser.Email = request.Email ?? loginUser.Email;
        await AppDbContext.SaveChangesAsync();
        var track = AppDbContext.Users.Update(loginUser);
        return Ok(track.Entity);
    }

    public class EditRequestPassword
    {
        public required Guid Id { get; set; }
        [MaxLength(64)] [MinLength(8)] public required string OldPassword { get; set; }
        [MaxLength(64)] [MinLength(8)] public required string NewPassword { get; set; }
    }

    [HttpPut("password")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [EndpointSummary("用户更新密码")]
    public async Task<IActionResult> EditUserPassword([FromBody] EditRequestPassword request)
    {
        var userId = this.GetLoginUserId();
        var loginUser = await AppDbContext.Users.SingleOrDefaultAsync(b => b.Id == request.Id);
        if (loginUser is null)
            return NotFound(new ProblemDetails() { Title = "用户不存在" });
        if (!PasswordHelper.CheckPassword(request.OldPassword, loginUser.Password))
            return Conflict(new ProblemDetails() { Title = "旧密码错误" });
        loginUser.Password = PasswordHelper.HashPassword(request.NewPassword);
        await AppDbContext.SaveChangesAsync();
        var track = AppDbContext.Users.Update(loginUser);
        return Ok(track.Entity);
    }

    public class EditRequestRole
    {
        public required Guid Id { get; set; }
        [Required] public required string Role { get; set; }
    }

    [HttpPut("role")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员修改用户角色")]
    public async Task<IActionResult> EditUserRole([FromBody] EditRequestRole request)
    {
        var userId = this.GetLoginUserId();
        var loginUser = await AppDbContext.Users.SingleOrDefaultAsync(b => b.Id == request.Id);
        if (loginUser is null)
            return NotFound(new ProblemDetails() { Title = "用户不存在" });
        loginUser.Role = request.Role;
        await AppDbContext.SaveChangesAsync();
        var track = AppDbContext.Users.Update(loginUser);
        return Ok(track.Entity);
    }

    #endregion

    #region Delete

    // 用户不能自己注销
    [HttpDelete("{id:guid}")]
    // [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("管理员删除用户")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await AppDbContext.Users.SingleOrDefaultAsync(x => x.Id == id);
        if (user is null)
            return NotFound(new ProblemDetails() { Title = "用户不存在" });
        var complaints = AppDbContext.Complaints.Where(x => x.SendUserId == id);
        AppDbContext.Complaints.RemoveRange(complaints);
        complaints = AppDbContext.Complaints.Where(x => x.ReceiveUserId == id);
        AppDbContext.Complaints.RemoveRange(complaints);
        complaints = AppDbContext.Complaints.Where(x => x.HandleUserId == id);
        AppDbContext.Complaints.RemoveRange(complaints);
        var violation = AppDbContext.Violations.Where(x => x.UserId == id);
        AppDbContext.Violations.RemoveRange(violation);
        var booking = AppDbContext.Bookings.Where(x => x.UserId == id);
        AppDbContext.Bookings.RemoveRange(booking);
        AppDbContext.Users.Remove(user);
        await AppDbContext.SaveChangesAsync();
        return Ok();
    }

    #endregion

    // #region 锁定用户
    //
    // [HttpPost("block")]
    // [Authorize(AuthorizationHelper.Policy.Admin)]
    // [EndpointSummary("锁定用户")]
    // public async Task<IActionResult> Block()
    // {
    //     public required Guid Id { get; set; }
    //     [MaxLength(64)]
    //     [MinLength(8)]
    //     public required string NewPassword { get; set; }
    // }
    //
    //
    //
    // #endregion

    #region Get

    [HttpGet("all")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("管理员获取所有用户")]
    public async Task<IActionResult> GetAllUsers([FromQuery] int page=1 , [FromQuery] int pageSize = 20)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 1;
        if (pageSize > 100)
            pageSize = 100; // 限制最大页大小

        var query = AppDbContext.Users.AsQueryable();

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreateTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync();

        return Ok(
            new
            {
                total,
                page,
                pageSize,
                items
            }
        );
    }

    #endregion
}