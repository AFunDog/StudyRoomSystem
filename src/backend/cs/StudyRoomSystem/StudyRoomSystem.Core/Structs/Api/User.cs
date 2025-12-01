using System.ComponentModel.DataAnnotations;

namespace StudyRoomSystem.Core.Structs.Api;

public class RegisterRequest
{
    [MaxLength(64)]
    [MinLength(4)]
    public required string UserName { get; set; }

    [MaxLength(64)]
    [MinLength(8)]
    public required string Password { get; set; }

    public string? DisplayName { get; set; }

    [MaxLength(64)]
    [MinLength(4)]
    public required string CampusId { get; set; }

    [Phone]
    public required string Phone { get; set; }

    [EmailAddress]
    public string? Email { get; set; }
}