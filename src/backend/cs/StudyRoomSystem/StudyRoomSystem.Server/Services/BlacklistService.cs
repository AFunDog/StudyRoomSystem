using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;

namespace StudyRoomSystem.Server.Services;

internal class BlacklistService(AppDbContext appDbContext) : IBlacklistService
{
    private AppDbContext AppDbContext { get; } = appDbContext;

    public async Task<IEnumerable<Blacklist>> GetValidBlacklists(Guid userId)
    {
        var blacklists = await AppDbContext
            .Blacklists.AsNoTracking()
            .Where(x => x.UserId == userId)
            .Where(x => DateTime.UtcNow < x.ExpireTime)
            .ToListAsync();
        return blacklists;
    }
}