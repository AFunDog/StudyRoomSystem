using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

public interface IViolationService
{
    Task<ApiPageResult<Violation>> GetAllByUserId(Guid userId, int page, int pageSize);
    Task<ApiPageResult<Violation>> GetAll(int page, int pageSize);
    Task<Violation> GetById(Guid violationId);
    Task<Violation> Create(Violation violation);
    Task<Violation> Update(Violation violation);
    Task<Violation> Delete(Guid id);
}
