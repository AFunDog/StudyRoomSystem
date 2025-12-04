using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.Core.Structs;

namespace StudyRoomSystem.AvaloniaApp.Services;

internal sealed partial class UserProvider : IUserProvider
{
    public User? User { get; set; }
}
