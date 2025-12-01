using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
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
    [ProducesResponseType<ResponseError>(StatusCodes.Status409Conflict)]
    [EndpointSummary("用户注册")]
    [EndpointDescription("用户需要使用该接口注册，注册成功之后需要使用用户名密码登录")]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request)
    {
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName)) is not null)
        {
            return Conflict(
                new ResponseError()
                {
                    Message = "用户名已存在"
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
                new ResponseError()
                {
                    Message = "用户注册失败"
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
    [ProducesResponseType<ResponseError>(StatusCodes.Status409Conflict)]
    [EndpointSummary("管理员注册")]
    [EndpointDescription("管理员需要使用该接口注册，注册成功之后需要使用用户名密码登录")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterRequest request)
    {
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.UserName == request.UserName)) is not null)
        {
            return Conflict(
                new ResponseError()
                {
                    Message = "用户名已存在"
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
                new ResponseError()
                {
                    Message = "用户注册失败"
                }
            );
        }
    }

    #endregion
    

    #region Edit

    public class EditRequest
    {
        
    }
    

    // TODO
    [HttpPut]
    [Authorize]
    [EndpointSummary("更新用户基本信息")]
    public async Task<IActionResult> EditUser(EditRequest request)
    {
        return Ok();
    }

    #endregion

    #region Delete
    
    // 用户不能自己注销
    [HttpDelete]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("管理员删除用户")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        return Ok();
    }
    
    #endregion

    #region 锁定用户

    [HttpPost("block")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [EndpointSummary("锁定用户")]
    public async Task<IActionResult> Block()
    {
        return Ok();
    }
    


    #endregion
}