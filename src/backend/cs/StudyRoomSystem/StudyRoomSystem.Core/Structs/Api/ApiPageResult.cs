namespace StudyRoomSystem.Core.Structs.Api;

public record ApiPageResult<T>
{
    public required int Total { get; set; }
    public required int Page { get; set; }
    public required int PageSize { get; set; }
    public required List<T> Items { get; set; } = [];
}
