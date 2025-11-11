using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using StudyRoomSystem.Server.Helpers;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.Server.Hubs;

public class DataHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Log
            .Logger.Trace()
            .Information(
                "Hub连接 {Id} {User} {UserName}",
                Context.ConnectionId,
                Context.UserIdentifier,
                Context.User?.FindFirst(ClaimTypes.Name)?.Value
            );
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Log
            .Logger.Trace()
            .Information(
                "Hub断开 {Id} {User} {UserName}",
                Context.ConnectionId,
                Context.UserIdentifier,
                Context.User?.FindFirst(ClaimTypes.Name)?.Value
            );
    }
}