using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Services;

internal sealed class AuthService(
    AppDbContext appDbContext,
    IUserService userService,
    IBlacklistService blacklistService) : IAuthService
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IUserService UserService { get; } = userService;
    private IBlacklistService BlacklistService { get; } = blacklistService;

    public async Task<User> Login(string userName, string password)
    {
        var user = await UserService.GetUserByUserName(userName);
        if (PasswordHelper.CheckPassword(password, user.Password) is false)
            throw new UnauthorizedException("密码错误");

        var blacklists = (await BlacklistService.GetAllValidByUserId(user.Id, 1, 1));
        if (blacklists.Total != 0)
        {
            throw new UnauthorizedException(
                "黑名单用户禁止登录",
                new Dictionary<string, object?> { ["blacklist"] = blacklists }
            );
        }

        return user;
    }
}