using System.Net;

namespace StudyRoomSystem.Core.Structs.Exceptions;

public class HttpResponseException(
    string title,
    string? message = null,
    IDictionary<string, object?>? extension = null,
    HttpStatusCode statusCode = HttpStatusCode.InternalServerError
    ) : Exception(message)
{
    public string Title { get; set; } = title;
    public IDictionary<string, object?> Extension { get; } = extension ?? new Dictionary<string, object?>();
    public HttpStatusCode StatusCode { get; set; } = statusCode;
}