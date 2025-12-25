using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

/// <summary>
/// 违纪记录服务接口，提供违纪记录的查询、创建、更新与删除等操作。
/// </summary>
public interface IViolationService
{
    /// <summary>
    /// 获取指定用户的违纪记录（分页）。
    /// </summary>
    Task<ApiPageResult<Violation>> GetAllByUserId(Guid userId, int page, int pageSize);

    /// <summary>
    /// 获取所有违纪记录（分页）。
    /// </summary>
    Task<ApiPageResult<Violation>> GetAll(int page, int pageSize);

    /// <summary>
    /// 根据 Id 获取单条违纪记录。
    /// </summary>
    Task<Violation> GetById(Guid violationId);

    /// <summary>
    /// 创建新的违纪记录。
    /// </summary>
    Task<Violation> Create(Violation violation);

    /// <summary>
    /// 更新已有的违纪记录。
    /// </summary>
    Task<Violation> Update(Violation violation);

    /// <summary>
    /// 删除指定的违纪记录。
    /// </summary>
    Task<Violation> Delete(Guid id);
}
