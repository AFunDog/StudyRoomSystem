using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Server.Controllers.Filters;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using StudyRoomSystem.Server.Structs;

namespace StudyRoomSystem.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/auth")]
[ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private IConfiguration Configuration { get; }
    private AppDbContext AppDbContext { get; }

    public AuthController(IConfiguration configuration, AppDbContext appDbContext)
    {
        Configuration = configuration;
        AppDbContext = appDbContext;
    }

    #region Login

    public class LoginRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponseOk
    {
        public required string Token { get; set; }
        public required DateTime Expiration { get; set; }
        public required User User { get; set; }
    }

    
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponseOk>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await AppDbContext.Users.SingleOrDefaultAsync(x => x.UserName == request.UserName);
        if (user is null)
            return Unauthorized(
                new ResponseError
                {
                    Message = "用户不存在"
                }
            );

        if (PasswordHelper.CheckPassword(request.Password, user.Password) is false)
            return Unauthorized();

        var claims = new List<Claim>()
        {
            new Claim(ClaimExtendTypes.Id, user.Id.ToString()), new Claim(ClaimTypes.Name, request.UserName),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]!);
        var cred = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Configuration["Jwt:Issuer"],
            audience: Configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(Configuration["Jwt:ExpireMinutes"])),
            signingCredentials: cred
        );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(
            new LoginResponseOk
            {
                Token = tokenString,
                Expiration = token.ValidTo,
                User = user
            }
        );
    }

    #endregion

    [HttpGet("check")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Check()
    {
        var userId = this.GetLoginUserId();
        if (userId == Guid.Empty)
            return Unauthorized();
        return Ok();
    }

    #region Register

    public class RegisterRequest
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }

        public string? DisplayName { get; set; }

        public required string CampusId { get; set; }

        [Phone]
        public required string Phone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status409Conflict)]
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

        var addUser = await AppDbContext.Users.AddAsync(
            new User
            {
                Id = Ulid.NewUlid().ToGuid(),
                UserName = request.UserName,
                Password = PasswordHelper.HashPassword(request.Password),
                DisplayName = request.DisplayName ?? request.UserName,
                CampusId = request.CampusId,
                Phone = request.Phone,
                Email = request.Email,
                Role = "User"
            }
        );

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

    [HttpPost("registerAdmin")]
    [Authorize(AuthorizationHelper.Policy.Admin)]
    [ProducesResponseType<User>(StatusCodes.Status200OK)]
    [ProducesResponseType<ResponseError>(StatusCodes.Status409Conflict)]
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

        var addUser = await AppDbContext.Users.AddAsync(
            new User
            {
                Id = Ulid.NewUlid().ToGuid(),
                UserName = request.UserName,
                Password = PasswordHelper.HashPassword(request.Password),
                DisplayName = request.DisplayName ?? request.UserName,
                CampusId = request.CampusId,
                Phone = request.Phone,
                Email = request.Email,
                Role = "Admin"
            }
        );

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
}