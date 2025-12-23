using System.Net;

namespace StudyRoomSystem.Core.Structs.Exceptions;

public sealed class NotFoundException(string message, IDictionary<string, object?>? extension = null)
    : HttpResponseException("未找到",message, extension, statusCode: HttpStatusCode.NotFound);
