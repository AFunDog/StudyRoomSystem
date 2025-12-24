using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using NotFoundException = StudyRoomSystem.Core.Structs.Exceptions.NotFoundException;

namespace StudyRoomSystem.Server.Services;

internal sealed class ViolationService(AppDbContext appDbContext) : IViolationService
{
    private AppDbContext AppDbContext { get; } = appDbContext;

    public async Task<ApiPageResult<Violation>> GetAllByUserId(Guid userId, int page, int pageSize)
    {
        return await AppDbContext
            .Violations.AsNoTracking()
            .Include(x => x.Booking)
            .Where(x => x.UserId == userId)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<ApiPageResult<Violation>> GetAll(int page, int pageSize)
    {
        return await AppDbContext
            .Violations.AsNoTracking()
            .Include(x => x.Booking)
            .Include(x => x.User)
            .ToApiPageResult(page, pageSize);
    }

    public async Task<Violation> GetById(Guid violationId)
    {
        return await AppDbContext
                   .Violations.AsNoTracking()
                   .Include(x => x.Booking)
                   .Include(x => x.User)
                   .SingleOrDefaultAsync(x => x.Id == violationId)
               ?? throw new NotFoundException("违规记录不存在");
    }

    public async Task<Violation> Create(Violation violation)
    {
        var containUser = await AppDbContext.Users.AnyAsync(x => x.Id == violation.UserId);
        if (containUser is false)
            throw new NotFoundException("违规用户不存在");

        if (violation.BookingId is not null)
        {
            var containBooking = await AppDbContext.Bookings.AnyAsync(x => x.Id == violation.UserId);
            if (containBooking is false)
                throw new NotFoundException("预约记录不存在");
        }

        var track = await AppDbContext.Violations.AddAsync(violation);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("违规记录添加失败");
        return track.Entity;
    }

    public async Task<Violation> Update(Violation violation)
    {
        var track = AppDbContext.Violations.Update(violation);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("违规记录更新失败");
        return track.Entity;
    }

    public async Task<Violation> Delete(Guid id)
    {
        var violation = await GetById(id);
        AppDbContext.Violations.Remove(violation);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("违规记录删除失败");
        return violation;
    }
}