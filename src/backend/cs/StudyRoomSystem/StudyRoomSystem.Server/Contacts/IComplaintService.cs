using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

/// <summary>
/// 投诉服务接口，提供投诉的查询、创建、处理与关闭等操作。
/// </summary>
public interface IComplaintService
{
    /// <summary>
    /// 分页获取所有投诉
    /// </summary>
    Task<ApiPageResult<Complaint>> GetAll(int page, int pageSize);

    /// <summary>
    /// 根据 Id 获取单条投诉
    /// </summary>
    Task<Complaint> GetById(Guid id);

    /// <summary>
    /// 分页获取指定发送者的投诉
    /// </summary>
    Task<ApiPageResult<Complaint>> GetAllBySendUserId(Guid userId, int page, int pageSize);

    /// <summary>
    /// 分页获取指定处理人的投诉
    /// </summary>
    Task<ApiPageResult<Complaint>> GetAllByHandleUserId(Guid userId, int page, int pageSize);

    /// <summary>
    /// 创建新的投诉记录
    /// </summary>
    Task<Complaint> Create(Complaint complaint);

    /// <summary>
    /// 更新投诉记录
    /// </summary>
    Task<Complaint> Update(Complaint complaint);

    /// <summary>
    /// 处理投诉并可创建相应违纪记录与评分
    /// </summary>
    Task<Complaint> Handle(
        Guid complaintId,
        Guid handleUserId,
        Guid targetUserId,
        string handleContent,
        string violationContent,
        int score);

    /// <summary>
    /// 关闭投诉记录
    /// </summary>
    Task<Complaint> Close(Guid complaintId, Guid handleUserId, string handleContent);

    /// <summary>
    /// 删除投诉（通常由权限或发送者执行）
    /// </summary>
    Task Delete(Guid complaintId,Guid userId);
}