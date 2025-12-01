namespace StudyRoomSystem.Core.Structs.Api;

public class LoginRequest
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
}

public class LoginResponseOk2
{
    public required DateTime Expiration { get; set; }
    public required User User { get; set; }
}
public class LoginResponseOk
{
    public required string Token { get; set; }
    public required DateTime Expiration { get; set; }
    public required User User { get; set; }
}