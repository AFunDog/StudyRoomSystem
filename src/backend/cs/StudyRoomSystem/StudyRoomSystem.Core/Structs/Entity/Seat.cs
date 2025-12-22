namespace StudyRoomSystem.Core.Structs.Entity;

public record Seat
{
    public Guid Id { get; set; } = Ulid.NewUlid().ToGuid();
    public required Guid RoomId { get; set; }
    public required int Row { get; set; }
    public required int Col { get; set; }

    public virtual Room Room { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = [];
}