using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.AvaloniaApp.Services;

internal sealed partial class UserProvider : IUserProvider
{
    public User? User { get; set; }
}
