using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace StudyRoomSystem.Server.Hubs;

[Authorize]
public class DataHub : Hub
{
    
}
