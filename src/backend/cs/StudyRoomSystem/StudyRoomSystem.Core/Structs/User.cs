using System.ComponentModel.DataAnnotations;

namespace StudyRoomSystem.Core.Structs;

public class User
{
    public required Guid Id { get; set; }
    
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    
    [MaxLength(64)]
    public required string UserName { get; set; }
    
    [MaxLength(128)]
    public required string DisplayName { get; set; }
    
    [MaxLength(512)]
    public required string Password { get; set; }
    
    [MaxLength(64)]
    public required string CampusId { get; set; }
    
    [MaxLength(64)]
    [Phone]
    public required string Phone { get; set; }
    
    [MaxLength(64)]
    [EmailAddress]
    public string? Email { get; set; }
    
    [MaxLength(64)]
    public required string Role { get; set; }
}
