using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Serilog;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using Zeng.CoreLibrary.Toolkit.Logging;
using NotFoundException = StudyRoomSystem.Core.Structs.Exceptions.NotFoundException;


namespace StudyRoomSystem.Server.Services;

internal class UserService(AppDbContext appDbContext) : IUserService
{
    private AppDbContext AppDbContext { get; } = appDbContext;

    public async Task<User> GetUserById(Guid userId)
    {
        var user = await AppDbContext.Users.SingleOrDefaultAsync(x => x.Id == userId);
        if (user is null)
            throw new NotFoundException("用户不存在");
        return user;
    }

    public async Task<ApiPageResult<User>> GetUsers(int page, int pageSize)
    {
        Guard.Against.OutOfRange(page, nameof(page), 1, int.MaxValue, "页码必须大于0");
        Guard.Against.OutOfRange(pageSize, nameof(pageSize), 1, 100, "页大小必须在1到100");

        var query = AppDbContext.Users.AsQueryable();

        var total = await query.CountAsync();

        var items = await query.OrderByDescending(u => u.CreateTime).Page(page, pageSize).ToListAsync();

        return new ApiPageResult<User>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }

    public async Task CheckUserInfoValid(User user)
    {
        // 检查用户名是否已存在
        if (await AppDbContext.Users.FirstOrDefaultAsync(x => x.UserName == user.UserName) is not null)
            throw new ConflictException("用户名已存在");

        // 检查学号/工号是否已存在
        if (await AppDbContext.Users.FirstOrDefaultAsync(x => x.CampusId == user.CampusId) is not null)
            throw new ConflictException("学号/工号已存在");

        // 检查手机号是否已存在
        if ((await AppDbContext.Users.FirstOrDefaultAsync(x => x.Phone == user.Phone)) is not null)
            throw new ConflictException("手机号已存在");

        // 检查邮箱是否已存在
        if (await AppDbContext.Users.FirstOrDefaultAsync(x => !string.IsNullOrEmpty(x.Email) && x.Email == user.Email)
            is not null)
            throw new ConflictException("邮箱已存在");
    }
    
    public async Task<User> RegisterUser(User user)
    {
        await CheckUserInfoValid(user);

        var track =  await AppDbContext.Users.AddAsync(user);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("用户注册失败");
        return track.Entity;
    }

    public async Task DeleteUser(Guid userId)
    {
        var user = await GetUserById(userId);
        AppDbContext.Users.Remove(user);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("用户删除失败");
    }
    
    public async Task<User> UpdateUser(User user)
    { 
        // var oldUser = await GetUserById(user.Id);
        // oldUser.DisplayName = user.DisplayName;
        // oldUser.Email = user.Email;
        // oldUser.Phone = user.Phone;
        // oldUser.Role = user.Role;
        // oldUser.Avatar = user.Avatar;
        var track = AppDbContext.Users.Update(user);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("用户更新失败");
        return track.Entity;
    }
}