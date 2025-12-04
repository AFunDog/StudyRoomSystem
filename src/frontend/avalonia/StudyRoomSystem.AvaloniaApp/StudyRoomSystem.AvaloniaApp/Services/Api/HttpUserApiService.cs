using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;

namespace StudyRoomSystem.AvaloniaApp.Services.Api;

internal sealed partial class HttpUserApiService : IUserApiService
{
    private ILogger Logger { get; }
    private IHttpClientFactory HttpClientFactory { get; }

    public HttpUserApiService(ILogger logger, IHttpClientFactory httpClientFactory)
    {
        Logger = logger.ForContext<HttpAuthApiService>();
        HttpClientFactory = httpClientFactory;
    }

    public async Task Register(
        RegisterRequest request,
        Func<HttpResponseMessage, User, Task>? onOk,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.PostAsync(
            "/api/v1/user/register",
            new StringContent(
                JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.RegisterRequest),
                Encoding.UTF8,
                "application/json"
            )
        );
        if (res.IsSuccessStatusCode)
        {
            if (onOk is null)
                return;
            var user = JsonSerializer.Deserialize<User>(
                await res.Content.ReadAsStringAsync(),
                AppJsonSerializerContext.Default.User
            );
            ArgumentNullException.ThrowIfNull(user);
            await onOk(res, user);
        }
        else
        {
            if (onError is null)
                return;
            var error = JsonSerializer.Deserialize<ProblemDetails>(
                await res.Content.ReadAsStringAsync(),
                AppJsonSerializerContext.Default.ProblemDetails
            );
            ArgumentNullException.ThrowIfNull(error);
            await onError(res, error);
        }
    }
}