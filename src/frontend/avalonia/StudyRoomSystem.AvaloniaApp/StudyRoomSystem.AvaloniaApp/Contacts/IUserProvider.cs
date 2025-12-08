using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.AvaloniaApp.Contacts;

public interface IUserProvider
{
    User? User { get; set; }
}
