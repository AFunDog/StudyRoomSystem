using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

/// <summary>
/// 用户服务接口，提供用户信息查询、校验与注册等操作。
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 根据用户 Id 获取用户实体。
    /// </summary>
    Task<User> GetUserById(Guid userId);

    /// <summary>
    /// 分页获取用户列表。
    /// </summary>
    Task<ApiPageResult<User>> GetUsers(int page, int pageSize);

    /// <summary>
    /// 校验用户信息是否合法；若不合法应抛出异常。
    /// </summary>
    Task CheckUserInfoValid(User user);

    /// <summary>
    /// 注册新用户并返回创建后的用户实体。
    /// </summary>
    Task<User> RegisterUser(User user);


    Task DeleteUser(Guid userId);
    Task<User> UpdateUser(User user);
    Task<User> GetUserByUserName(string userName);
}
