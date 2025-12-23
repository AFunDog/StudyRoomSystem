using System.Net;

namespace StudyRoomSystem.Core.Structs.Exceptions;

public class UnauthorizedException(string? message = null, IDictionary<string, object?>? extension = null)
    : HttpResponseException("未授权",message ?? "未授权或者未登录", extension, statusCode: HttpStatusCode.Conflict);
