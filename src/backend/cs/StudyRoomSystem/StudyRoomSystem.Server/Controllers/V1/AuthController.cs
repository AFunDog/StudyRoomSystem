using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Controllers.Filters;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthController(IConfiguration configuration, AppDbContext appDbContext,IBlacklistService blacklistService) : ControllerBase
{
    private IConfiguration Configuration { get; } = configuration;
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IBlacklistService BlacklistService { get; } = blacklistService;

    // public static IEndpointRouteBuilder MapAuthController(this IEndpointRouteBuilder builder)
    // {
    //     var group = builder.MapGroup("api/v{version:apiVersion}/auth");
    //     group.MapGet("l", () => { });
    //     return builder;
    // }


    #region Login

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponseOk>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("用户登录")]
    [EndpointDescription("登陆后会自动写入Cookie")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await AppDbContext.Users.SingleOrDefaultAsync(x => x.UserName == request.UserName);
        if (user is null)
            return Unauthorized(
                new ProblemDetails()
                {
                    Title = "用户不存在"
                }
            );

        if (PasswordHelper.CheckPassword(request.Password, user.Password) is false)
            return Unauthorized(
                new ProblemDetails()
                {
                    Title = "密码错误"
                }
            );
        
        // 检查黑名单
        var blacklists = (await BlacklistService.GetValidBlacklists(user.Id)).ToArray();
        if (blacklists.Any())
        {
            return Forbid();
        }
        
        var claims = new List<Claim>()
        {
            new Claim(ClaimExtendTypes.Id, user.Id.ToString()), new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!);
        var cred = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var minutes = double.TryParse(Configuration["Jwt:ExpireMinutes"], out var mins) ? mins : 7 * 24 * 60;
        var token = new JwtSecurityToken(
            issuer: Configuration["Jwt:Issuer"],
            audience: Configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: cred
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        Response.Cookies.Append(
            AuthorizationHelper.CookieKey,
            tokenString,
            new CookieOptions()
            {
                MaxAge = TimeSpan.FromMinutes(minutes),
                IsEssential = true
            }
        );
        Log.Logger.Trace().Information("用户登录 {Id}", user);
        return Ok(
            new LoginResponseOk
            {
                Expiration = token.ValidTo,
                User = user
            }
        );
    }

    #endregion

    #region Logout

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("用户登出")]
    public async Task<IActionResult> Logout()
    {
        var user = this.GetLoginUserId();
        if (user == Guid.Empty)
            return Unauthorized();

        Response.Cookies.Delete(AuthorizationHelper.CookieKey);
        Log.Logger.Trace().Information("用户登出 {Id}", user);
        return Ok();
    }

    #endregion

    [HttpGet("check")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EndpointSummary("检查登录Token是否有效")]
    [EndpointDescription("此接口用于检查登录Token是否失效或者不处于登录状态")]
    public async Task<IActionResult> Check()
    {
        var userId = this.GetLoginUserId();
        if (userId == Guid.Empty)
            return Unauthorized();
        return Ok();
    }
}