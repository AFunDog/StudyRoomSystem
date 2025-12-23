using System.Net;

namespace StudyRoomSystem.Core.Structs.Exceptions;

public sealed class ConflictException(string message, IDictionary<string, object?>? extension = null)
    : HttpResponseException("冲突",message, extension, statusCode: HttpStatusCode.Conflict);