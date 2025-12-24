using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Helpers;

public static class UserExtension
{

    public static TimeSpan GetBookingLimit(this User user)
    {
        if (user.Credits < 40)
            return TimeSpan.Zero;
        if (user.Credits < 60)
            return TimeSpan.FromHours(2);
        return TimeSpan.FromDays(1);
    }
}