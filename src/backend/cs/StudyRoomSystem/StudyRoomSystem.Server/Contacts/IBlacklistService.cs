using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

/// <summary>
/// 黑名单服务接口，提供黑名单的查询、创建、更新与删除等操作。
/// </summary>
public interface IBlacklistService
{
    /// <summary>
    /// 分页获取指定用户的黑名单记录
    /// </summary>
    Task<ApiPageResult<Blacklist>> GetAllByUserId(Guid userId, int page, int pageSize);

    /// <summary>
    /// 分页获取所有黑名单记录
    /// </summary>
    Task<ApiPageResult<Blacklist>> GetAll(int page, int pageSize);

    /// <summary>
    /// 根据 Id 获取单条黑名单记录
    /// </summary>
    Task<Blacklist> GetById(Guid id);

    /// <summary>
    /// 创建新的黑名单记录
    /// </summary>
    Task<Blacklist> Create(Blacklist blacklist);

    /// <summary>
    /// 更新已有的黑名单记录
    /// </summary>
    Task<Blacklist> Update(Blacklist blacklist);

    /// <summary>
    /// 删除指定黑名单记录
    /// </summary>
    Task Delete(Guid id);

    /// <summary>
    /// 分页获取指定用户的有效黑名单记录
    /// </summary>
    Task<ApiPageResult<Blacklist>> GetAllValidByUserId(Guid userId, int page, int pageSize);

    /// <summary>
    /// 分页获取所有有效的黑名单记录
    /// </summary>
    Task<ApiPageResult<Blacklist>> GetAllValid(int page, int pageSize);
}