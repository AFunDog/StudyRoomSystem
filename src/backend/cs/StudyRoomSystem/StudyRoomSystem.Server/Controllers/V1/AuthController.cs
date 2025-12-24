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
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Controllers.Filters;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthController(
    IConfiguration configuration,
    AppDbContext appDbContext,
    IUserService userService,
    IBlacklistService blacklistService) : ControllerBase
{
    private IConfiguration Configuration { get; } = configuration;
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IUserService UserService { get; } = userService;
    private IBlacklistService BlacklistService { get; } = blacklistService;

    #region Login

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponseOk>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("用户登录")]
    [EndpointDescription("登陆后会自动写入Cookie")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await UserService.GetUserByUserName(request.UserName);

        if (PasswordHelper.CheckPassword(request.Password, user.Password) is false)
            throw new UnauthorizedException("密码错误");

        // 检查黑名单
        var blacklists = (await BlacklistService.GetAllValidByUserId(user.Id, 1, 1));
        if (blacklists.Total != 0)
        {
            throw new UnauthorizedException(
                "黑名单用户禁止登录",
                new Dictionary<string, object?> { ["blacklist"] = blacklists }
            );
        }

        var claims = new List<Claim>()
        {
            new Claim(ClaimExtendTypes.Id, user.Id.ToString()),
            new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!);
        var cred = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var expires = request.Long
            ? TimeSpan.FromHours(double.TryParse(Configuration["Jwt:ExpireHours"], out var hours) ? hours : 3)
            : TimeSpan.FromDays(double.TryParse(Configuration["Jwt:LongExpireDays"], out var days) ? days : 3);
        var token = new JwtSecurityToken(
            issuer: Configuration["Jwt:Issuer"],
            audience: Configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.Add(expires),
            signingCredentials: cred
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        Response.Cookies.Append(
            AuthorizationHelper.CookieKey,
            tokenString,
            new CookieOptions() { MaxAge = expires, IsEssential = true }
        );
        Log.Logger.Trace().Information("用户登录 {Id}", user);
        return Ok(new LoginResponseOk { Expiration = token.ValidTo, User = user });
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
        var user = await UserService.GetUserById(this.GetLoginUserId());

        Response.Cookies.Delete(AuthorizationHelper.CookieKey);
        Log.Logger.Trace().Information("用户登出 {Id}", user.Id);
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
        var user = await UserService.GetUserById(this.GetLoginUserId());
        return Ok();
    }
}