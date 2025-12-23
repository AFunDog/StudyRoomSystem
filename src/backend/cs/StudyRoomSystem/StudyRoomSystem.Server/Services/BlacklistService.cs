using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using NotFoundException = StudyRoomSystem.Core.Structs.Exceptions.NotFoundException;

namespace StudyRoomSystem.Server.Services;

internal class BlacklistService(AppDbContext appDbContext) : IBlacklistService
{
    private AppDbContext AppDbContext { get; } = appDbContext;

    public async Task<ApiPageResult<Blacklist>> GetAllByUserId(Guid userId, int page, int pageSize)
    {
        return await AppDbContext
            .Blacklists.AsNoTracking()
            .OrderBy(x => x.Id)
            .Where(x => x.UserId == userId)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<ApiPageResult<Blacklist>> GetAllValidByUserId(Guid userId, int page, int pageSize)
    {
        return await AppDbContext
            .Blacklists.AsNoTracking()
            .OrderBy(x => x.Id)
            .Where(x => x.UserId == userId)
            .Where(x => x.ExpireTime > DateTime.UtcNow)
            .ToApiPageResult(page, pageSize);
    }


    public async Task<ApiPageResult<Blacklist>> GetAll(int page, int pageSize)
    {
        return await AppDbContext.Blacklists.AsNoTracking().ToApiPageResult(page, pageSize);
    }

    public async Task<ApiPageResult<Blacklist>> GetAllValid(int page, int pageSize)
    {
        return await AppDbContext
            .Blacklists.AsNoTracking()
            .Where(x => x.ExpireTime > DateTime.UtcNow)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<Blacklist> GetById(Guid id)
    {
        var blacklist = await AppDbContext.Blacklists.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (blacklist is null)
            throw new NotFoundException("黑名单不存在");
        return blacklist;
    }

    public async Task<Blacklist> Create(Blacklist blacklist)
    {
        var track = await AppDbContext.Blacklists.AddAsync(blacklist);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("创建黑名单失败");
        return track.Entity;
    }

    public async Task<Blacklist> Update(Blacklist blacklist)
    {
        var track = AppDbContext.Blacklists.Update(blacklist);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("更新黑名单失败");
        return track.Entity;
    }

    public async Task Delete(Guid id)
    {
        var blacklist = await GetById(id);
        AppDbContext.Blacklists.Remove(blacklist);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("删除黑名单失败");
    }
}