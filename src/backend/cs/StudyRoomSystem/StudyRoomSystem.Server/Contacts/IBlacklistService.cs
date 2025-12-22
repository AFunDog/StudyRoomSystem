using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

public interface IBlacklistService
{
    Task<ApiPageResult<Blacklist>> GetAllByUserId(Guid userId, int page, int pageSize);

    Task<ApiPageResult<Blacklist>> GetAll(int page, int pageSize);
    Task<Blacklist> GetById(Guid id);
    Task<Blacklist> Create(Blacklist blacklist);
    Task<Blacklist> Update(Blacklist blacklist);
    Task Delete(Guid id);
    Task<ApiPageResult<Blacklist>> GetAllValidByUserId(Guid userId, int page, int pageSize);
    Task<ApiPageResult<Blacklist>> GetAllValid(int page, int pageSize);
}
