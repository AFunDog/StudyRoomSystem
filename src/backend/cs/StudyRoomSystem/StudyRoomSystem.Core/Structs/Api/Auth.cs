using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs.Api;

public class LoginRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}

public abstract class LoginResponse;

public sealed class LoginResponseOk : LoginResponse
{
    public required DateTime Expiration { get; set; }
    public required User User { get; set; }
}