using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

public interface IBlacklistService
{
    Task<IEnumerable<Blacklist>> GetValidBlacklists(Guid userId);
    
}
