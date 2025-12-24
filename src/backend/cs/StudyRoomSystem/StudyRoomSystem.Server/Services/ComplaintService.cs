using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Services;

internal sealed class ComplaintService(
    AppDbContext appDbContext,
    IUserService userService,
    IViolationService violationService) : IComplaintService
{
    private AppDbContext AppDbContext { get; } = appDbContext;
    private IUserService UserService { get; } = userService;
    private IViolationService ViolationService { get; } = violationService;

    public async Task<ApiPageResult<Complaint>> GetAll(int page, int pageSize)
    {
        return await AppDbContext
            .Complaints.AsNoTracking()
            .OrderByDescending(x => x.CreateTime)
            .Include(x => x.SendUser)
            .Include(x => x.HandleUser)
            .Include(x => x.Seat)
            .ThenInclude(x => x.Room)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<Complaint> GetById(Guid id)
    {
        var item = await AppDbContext
            .Complaints
            .AsNoTracking()
            .Include(x => x.Seat)
            .Include(x => x.SendUser)
            .Include(x => x.HandleUser)
            .Include(x => x.Seat)
            .ThenInclude(x => x.Room)
            .SingleOrDefaultAsync(x => x.Id == id);
        return item ?? throw new NotFoundException("投诉不存在");
    }

    public async Task<ApiPageResult<Complaint>> GetAllBySendUserId(Guid userId, int page, int pageSize)
    {
        return await AppDbContext
            .Complaints.AsNoTracking()
            .OrderByDescending(x => x.CreateTime)
            .Include(x => x.HandleUser)
            .Include(x => x.Seat)
            .ThenInclude(x => x.Room)
            .Where(x => x.SendUserId == userId)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<ApiPageResult<Complaint>> GetAllByHandleUserId(Guid userId, int page, int pageSize)
    {
        return await AppDbContext
            .Complaints.AsNoTracking()
            .OrderByDescending(x => x.CreateTime)
            .Include(x => x.SendUser)
            .Include(x => x.Seat)
            .ThenInclude(x => x.Room)
            .Where(x => x.HandleUserId == userId)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<Complaint> Create(Complaint complaint)
    {
        var track = await AppDbContext.Complaints.AddAsync(complaint);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("发起投诉失败");
        return track.Entity;
    }

    public async Task<Complaint> Update(Complaint complaint)
    {
        // var old = await GetById(complaint.Id);
        // if (old.State is not ComplaintStateEnum.已发起)
        //     throw new BadHttpRequestException("投诉状态不是已发起状态");

        var track = AppDbContext.Complaints.Update(complaint);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("修改投诉失败");
        return track.Entity;
    }

    public async Task<Complaint> Handle(
        Guid complaintId,
        Guid handleUserId,
        Guid targetUserId,
        string handleContent,
        string violationContent,
        int score)
    {
        var handleUser = await UserService.GetUserById(handleUserId);
        var targetUser = await UserService.GetUserById(targetUserId);
        var complaint = await GetById(complaintId);

        if (complaint.State is not ComplaintStateEnum.已发起)
            throw new BadHttpRequestException("投诉状态不是已发起状态");

        if (handleUser.Role is not UserRoleEnum.Admin)
            throw new ForbidException("权限不足");

        complaint.State = ComplaintStateEnum.已处理;
        complaint.HandleContent = handleContent;
        complaint.HandleTime = DateTime.UtcNow;
        complaint.HandleUserId = handleUser.Id;

        await using var transaction = await AppDbContext.Database.BeginTransactionAsync();

        await ViolationService.Create(
            new Violation()
            {
                UserId = targetUser.Id,
                Content = violationContent,
                State = ViolationStateEnum.Violation,
                Type = ViolationTypeEnum.管理员,
            }
        );

        await UserService.UpdateUser(targetUser with { Credits = targetUser.Credits - score });
        
        var res = await Update(complaint);
        await transaction.CommitAsync();
        return res;
    }

    public async Task<Complaint> Close(Guid complaintId, Guid handleUserId, string handleContent)
    {
        var handleUser = await UserService.GetUserById(handleUserId);
        var complaint = await GetById(complaintId);

        if (complaint.State is not ComplaintStateEnum.已发起)
            throw new BadHttpRequestException("投诉状态不是已发起状态");

        complaint.State = ComplaintStateEnum.已关闭;
        complaint.HandleContent = handleContent;
        complaint.HandleTime = DateTime.UtcNow;
        complaint.HandleUserId = handleUser.Id;

        return await Update(complaint);
    }

    public async Task Delete(Guid id)
    {
        var complaint = await AppDbContext.Complaints.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (complaint is null)
            throw new NotFoundException("投诉不存在");
        AppDbContext.Complaints.Remove(complaint);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("删除投诉失败");
    }
}