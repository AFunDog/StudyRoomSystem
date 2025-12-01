using StudyRoomSystem.AvaloniaApp.Contacts;

namespace StudyRoomSystem.AvaloniaApp.Services;

internal sealed partial class JsonWebTokenProvider : ITokenProvider
{
    public string? Token { get; set; } 
}
