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
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/user")]
[ApiVersion("1.0")]
public class UserController(AppDbContext appDbContext, IUserService userService) : ControllerBase
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IUserService UserService { get; } = userService;


    #region Register

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("用户注册")]
    [EndpointDescription("用户需要使用该接口注册，注册成功之后需要使用用户名密码登录")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        var user = await UserService.GetUserById(this.GetLoginUserId());

        // 检查用户权限
        if (request.Role == UserRoleEnum.Admin && user.Role != UserRoleEnum.Admin)
            throw new ForbidException("用户权限不足");

        return Ok(await UserService.RegisterUser(CreateUser(request)));
    }

    private static User CreateUser(RegisterRequest request) => new()
    {
        UserName = request.UserName,
        Password = PasswordHelper.HashPassword(request.Password),
        // 如果昵称为空，则默认使用用户名
        DisplayName = request.DisplayName ?? request.UserName,
        CampusId = request.CampusId,
        Phone = request.Phone,
        Email = request.Email,
        Role = request.Role
    };

    #endregion


    #region Edit

    public class EditRequestNormal
    {
        public required Guid Id { get; set; }
        [MaxLength(128)]
        public string? DisplayName { get; set; }
        [MaxLength(64)]
        public string? CampusId { get; set; }
        [MaxLength(64)]
        [Phone]
        public string? Phone { get; set; }
        [MaxLength(64)]
        [EmailAddress]
        public string? Email { get; set; }
    }

    // TODO
    [HttpPut("information")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [EndpointSummary("用户更新基本信息")]
    public async Task<IActionResult> EditUserNormal([FromBody] EditRequestNormal request)
    {
        var loginUser = await UserService.GetUserById(this.GetLoginUserId());
        var targetUser = await UserService.GetUserById(request.Id);

        // 如果不是管理员则只能修改自己的信息
        if (loginUser.Id != targetUser.Id && loginUser.Role != UserRoleEnum.Admin)
            throw new ForbidException("用户权限不足");

        targetUser.DisplayName = request.DisplayName ?? targetUser.DisplayName;
        targetUser.CampusId = request.CampusId ?? targetUser.CampusId;
        targetUser.Phone = request.Phone ?? targetUser.Phone;
        targetUser.Email = request.Email ?? targetUser.Email;

        return Ok(await UserService.UpdateUser(targetUser));
    }

    public class EditRequestPassword
    {
        public required Guid Id { get; set; }
        [MaxLength(64)]
        [MinLength(8)]
        public required string OldPassword { get; set; }
        [MaxLength(64)]
        [MinLength(8)]
        public required string NewPassword { get; set; }
    }

    [HttpPut("password")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [EndpointSummary("用户更新密码")]
    public async Task<IActionResult> EditUserPassword([FromBody] EditRequestPassword request)
    {
        var loginUser = await UserService.GetUserById(this.GetLoginUserId());
        var targetUser = await UserService.GetUserById(request.Id);

        // 如果不是管理员则只能修改自己的信息
        if (loginUser.Id != targetUser.Id && loginUser.Role != UserRoleEnum.Admin)
            throw new ForbidException("用户权限不足");
        
        // 如果不是管理员就要检验旧密码
        if (loginUser.Role is not UserRoleEnum.Admin)
        {
            if (!PasswordHelper.CheckPassword(request.OldPassword, targetUser.Password))
                throw new ConflictException("旧密码错误");
        }

        targetUser.Password = PasswordHelper.HashPassword(request.NewPassword);

        return Ok(await UserService.UpdateUser(targetUser));
    }

    public class EditRequestRole
    {
        public required Guid Id { get; set; }
        public required UserRoleEnum Role { get; set; }
    }

    [HttpPut("role")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员修改用户角色")]
    public async Task<IActionResult> EditUserRole([FromBody] EditRequestRole request)
    {
        var user = await UserService.GetUserById(request.Id);
        user.Role = request.Role;
        return Ok(await UserService.UpdateUser(user));
    }

    #endregion

    #region Delete

    // 用户不能自己注销
    [HttpDelete]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("管理员删除用户")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        await UserService.DeleteUser(id);
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

    [HttpGet]
    [Authorize]
    [EndpointSummary("获取登录用户的基本信息")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoginUser()
    {
        var userId = this.GetLoginUserId();
        if (userId == Guid.Empty)
            throw new UnauthorizedException();
        var user = await UserService.GetUserById(userId);
        return Ok(user);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [EndpointSummary("获取指定的用户信息")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await UserService.GetUserById(id);
        return Ok(user);
    }

    [HttpGet("all")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<ApiPageResult<User>>(StatusCodes.Status200OK)]
    [EndpointSummary("管理员获取所有用户")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] [Range(1, int.MaxValue)] int page = 1,
        [FromQuery] [Range(1, 100)] int pageSize = 20)
    {
        return Ok(await UserService.GetUsers(page, pageSize));
    }

    #endregion
}