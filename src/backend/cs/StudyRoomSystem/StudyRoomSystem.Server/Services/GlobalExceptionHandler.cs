using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Core.Structs.Exceptions;

namespace StudyRoomSystem.Server.Services;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, errorMessage) = exception switch
        {
            UnauthorizedException unauthorizedException => (StatusCodes.Status401Unauthorized, "未授权"),
            ForbidException forbidException => (StatusCodes.Status403Forbidden, "禁止访问"),
            NotFoundException notFoundException => (StatusCodes.Status404NotFound,"未找到"),
            ConflictException conflictException => (StatusCodes.Status409Conflict, "冲突"),
            _ => (StatusCodes.Status500InternalServerError, "服务器内部错误")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = errorMessage,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Extensions =
            {
                ["timestamp"] = DateTime.UtcNow,
                ["traceId"] = httpContext.TraceIdentifier
            }
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}