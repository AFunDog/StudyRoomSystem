using System.Net;

namespace StudyRoomSystem.Core.Structs.Exceptions;

public sealed class ForbidException(string message, IDictionary<string, object?>? extension = null)
    : HttpResponseException("禁止访问",message, extension, statusCode: HttpStatusCode.Forbidden);
