namespace StudyRoomSystem.Server.Helpers;

public static class AuthorizationHelper
{
    public static class Policy
    {
        public const string Admin = nameof(Admin);
        public const string User = nameof(User);
    }

    public static class Role
    {
        public const string Admin = nameof(Admin);
        public const string User = nameof(User);
    }
}