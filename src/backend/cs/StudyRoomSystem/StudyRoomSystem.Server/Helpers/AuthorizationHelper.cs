using System.Security.Claims;

namespace StudyRoomSystem.Server.Helpers;

public static class AuthorizationHelper
{
    public const string CookieKey = "AuthToken";
    
    public static class Policy
    {
        public const string Admin = nameof(Admin);
        public const string User = nameof(User);
    }
    
    public static Guid GetLoginUserId(this ClaimsPrincipal user)
    {
        var userId = Guid.TryParse(user.FindFirst(ClaimExtendTypes.Id)?.Value, out var id) ? id : Guid.Empty;
        return userId;
    }
}