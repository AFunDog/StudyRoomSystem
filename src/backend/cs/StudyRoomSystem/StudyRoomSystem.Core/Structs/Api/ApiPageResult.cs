namespace StudyRoomSystem.Core.Structs.Api;

public class ApiPageResult<T>
{
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<T> Items { get; set; } = [];
}
