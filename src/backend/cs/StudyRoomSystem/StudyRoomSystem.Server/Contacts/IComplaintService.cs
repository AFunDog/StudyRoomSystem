using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Server.Contacts;

public interface IComplaintService
{
    Task<ApiPageResult<Complaint>> GetAll(int page, int pageSize);
    Task<Complaint> GetById(Guid id);
    Task<ApiPageResult<Complaint>> GetAllBySendUserId(Guid userId, int page, int pageSize);
    Task<ApiPageResult<Complaint>> GetAllByHandleUserId(Guid userId, int page, int pageSize);
    Task<Complaint> Create(Complaint complaint);
    Task<Complaint> Update(Complaint complaint);

    Task<Complaint> Handle(
        Guid complaintId,
        Guid handleUserId,
        Guid targetUserId,
        string handleContent,
        string violationContent,
        int score);

    Task<Complaint> Close(Guid complaintId, Guid handleUserId, string handleContent);
    Task Delete(Guid complaintId,Guid userId);
}