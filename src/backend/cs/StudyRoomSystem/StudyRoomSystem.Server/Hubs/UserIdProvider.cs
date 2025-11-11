using Microsoft.AspNetCore.SignalR;
using StudyRoomSystem.Server.Helpers;

namespace StudyRoomSystem.Server.Hubs;

public sealed class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimExtendTypes.Id)?.Value;
    }
}
