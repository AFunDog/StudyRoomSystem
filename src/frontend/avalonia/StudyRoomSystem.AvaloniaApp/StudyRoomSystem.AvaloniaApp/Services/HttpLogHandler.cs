using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.AvaloniaApp.Services;

internal sealed partial class HttpLogHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var uri = request.RequestUri;
        try
        {
            Log.Logger.Trace().Information("HTTP 请求 {Method} {Uri}", request.Method, uri);
            var response = await base.SendAsync(request, cancellationToken);
            Log.Logger.Trace().Information("HTTP 请求-响应 {Method} {Uri} {E}ms {Res}", request.Method, uri,stopwatch.ElapsedMilliseconds, response.StatusCode);
            return response;
        }
        catch (Exception e)
        {
            Log.Logger.Trace().Error(e, "HTTP 错误 {Uri} {E}ms", uri, stopwatch.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}