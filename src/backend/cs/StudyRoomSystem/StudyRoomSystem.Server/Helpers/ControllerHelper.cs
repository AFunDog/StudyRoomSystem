using System;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Core.Structs;

namespace StudyRoomSystem.Server.Helpers;

public static class ControllerHelper
{
    public static Guid GetLoginUserId(this ControllerBase controller)
    {
        var userId = Guid.TryParse(controller.User.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        return userId;
    }
}
