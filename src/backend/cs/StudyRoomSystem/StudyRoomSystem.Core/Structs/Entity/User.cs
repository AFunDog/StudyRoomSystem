using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudyRoomSystem.Core.Structs.Entity;

[JsonConverter(typeof(JsonStringEnumConverter<UserRoleEnum>))]
public enum UserRoleEnum
{
    User,Admin
}

public class User
{
    public required Guid Id { get; set; }
    
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    
    [MinLength(4,ErrorMessage = "用户名至少4位")]
    [MaxLength(20,ErrorMessage = "用户名不能超过20位")]
    // TODO 用户名应该可以中文
    [RegularExpression("^[a-zA-Z0-9._]+$",ErrorMessage = "用户名只能包含字母、数字、点或下划线")]
    public required string UserName { get; set; }
    
    [MinLength(4,ErrorMessage = "昵称至少4位")]
    [MaxLength(20,ErrorMessage = "昵称不能超过20位")]
    public required string DisplayName { get; set; }
    
    [MinLength(8,ErrorMessage = "密码至少8位")]
    [MaxLength(32,ErrorMessage = "密码不能超过32位")]
    public required string Password { get; set; }
    
    [MaxLength(64)]
    public required string CampusId { get; set; }
    
    [MaxLength(64)]
    [Phone]
    // TODO Phone
    public required string Phone { get; set; }
    
    [MaxLength(64)]
    [EmailAddress]
    public string? Email { get; set; }
    
    public required UserRoleEnum Role { get; set; }
    
    [Url]
    public string? Avatar { get; set; }
}
