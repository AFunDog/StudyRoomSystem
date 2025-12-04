using StudyRoomSystem.Core.Structs;

namespace StudyRoomSystem.AvaloniaApp.Contacts;

public interface IUserProvider
{
    User? User { get; set; }
}
