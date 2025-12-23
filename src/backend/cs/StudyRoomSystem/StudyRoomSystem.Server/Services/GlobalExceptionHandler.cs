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
        var (statusCode, errorMessage, extension) = exception switch
        {
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "请求错误", null),
            HttpResponseException httpResponseException => ((int)httpResponseException.StatusCode,
                httpResponseException.Title, httpResponseException.Extension),
            _ => (StatusCodes.Status500InternalServerError, "服务器内部错误", null)
        };
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = errorMessage,
            Detail = exception.Message,
            Instance = httpContext.Request.Path,
            // Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Type = $"https://httpstatuses.com/{statusCode}",
        };
        if (extension is not null)
            problemDetails.Extensions = extension;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        
        
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}