using System.ComponentModel.DataAnnotations;
using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Core.Helpers;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Core.Structs.Exceptions;
using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Helpers;
using NotFoundException = StudyRoomSystem.Core.Structs.Exceptions.NotFoundException;

namespace StudyRoomSystem.Server.Services;

internal sealed class RoomService(AppDbContext appDbContext) : IRoomService
{
    private AppDbContext AppDbContext { get; } = appDbContext;

    public async Task<Seat> GetSeatById(Guid seatId)
    {
        var seat = await AppDbContext
            .Seats.AsNoTracking()
            .Include(x => x.Room)
            .SingleOrDefaultAsync(x => x.Id == seatId);
        if (seat is null)
            throw new NotFoundException("座位不存在");
        return seat;
    }

    public async Task<Room> GetRoomById(Guid roomId)
    {
        var room = await AppDbContext
            .Rooms.AsNoTracking()
            .Include(x => x.Seats)
            .SingleOrDefaultAsync(x => x.Id == roomId);
        if (room is null)
            throw new NotFoundException("房间不存在");
        return room;
    }

    public async Task<ApiPageResult<Room>> GetAllRoom(int page, int pageSize)
    {
        return await AppDbContext.Rooms.AsNoTracking().OrderBy(x => x.Id).ToApiPageResult(page, pageSize);
    }

    public async Task<Seat> AddSeat(Seat seat)
    {
        Guard.Against.NegativeOrZero(seat.Row);
        Guard.Against.NegativeOrZero(seat.Col);

        var room = await GetRoomById(seat.RoomId);
        if (room.HasSeat(seat.Row, seat.Col))
            throw new ConflictException("该位置座位已存在");

        var track = await AppDbContext.Seats.AddAsync(seat);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("座位添加失败");
        return track.Entity;
    }

    public async Task<Room> DeleteSeat(Guid seatId)
    {
        // TODO 需不需要检查是否有未结束的预约

        var seat = await GetSeatById(seatId);

        AppDbContext.Seats.Remove(seat);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("座位删除失败");
        return seat.Room;
    }

    public async Task<Room> CreateRoom(Room room)
    {
        if (room.OpenTime >= room.CloseTime)
            throw new ValidationException("房间开放时间不能晚于结束时间");
        
        var track = await AppDbContext.Rooms.AddAsync(room);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("房间添加失败");
        return track.Entity;
    }

    public async Task<Room> DeleteRoom(Guid roomId)
    {
        var room = await GetRoomById(roomId);
        AppDbContext.Rooms.Remove(room);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("房间删除失败");
        return room;
    }

    public async Task<Room> UpdateRoom(Room room)
    {
        AppDbContext.Rooms.Update(room);
        var res = await AppDbContext.SaveChangesAsync();
        if (res == 0)
            throw new ConflictException("房间更新失败");
        return room;
    }
    
}