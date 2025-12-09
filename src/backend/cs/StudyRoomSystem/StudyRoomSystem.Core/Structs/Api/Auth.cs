using System.ComponentModel.DataAnnotations;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.Core.Structs.Api;

public class LoginRequest
{
    [MinLength(4,ErrorMessage = "用户名至少4位")]
    [MaxLength(20,ErrorMessage = "用户名不能超过20位")]
    // TODO 用户名应该可以中文
    [RegularExpression("^[a-zA-Z0-9._]+$",ErrorMessage = "用户名只能包含字母、数字、点或下划线")]
    public required string UserName { get; set; }
    
    [MinLength(8,ErrorMessage = "密码至少8位")]
    [MaxLength(32,ErrorMessage = "密码不能超过32位")]
    public required string Password { get; set; }
}

public sealed class LoginResponseOk 
{
    public required DateTime Expiration { get; set; }
    public required User User { get; set; }
}