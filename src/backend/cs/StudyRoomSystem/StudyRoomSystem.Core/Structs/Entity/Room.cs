using System.ComponentModel.DataAnnotations;

namespace StudyRoomSystem.Core.Structs.Entity;

public class Room
{
    public Guid Id { get; set; } = Ulid.NewUlid().ToGuid();
    
    [MaxLength(64)]
    public required string Name { get; set; }
    
    public required TimeOnly OpenTime { get; set; }
    
    public required TimeOnly CloseTime { get; set; }
    
    [Range(1,2048)]
    public required int Rows { get; set; }
    
    [Range(1,2048)]
    public required int Cols { get; set; }

    public virtual ICollection<Seat> Seats { get; set; } = [];
}
