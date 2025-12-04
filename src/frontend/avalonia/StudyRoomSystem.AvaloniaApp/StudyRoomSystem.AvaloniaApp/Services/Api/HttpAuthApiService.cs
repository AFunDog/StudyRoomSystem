using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using ShadUI;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.Core.Structs.Api;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.AvaloniaApp.Services.Api;

internal sealed partial class HttpAuthApiService : IAuthApiService
{
    private ILogger Logger { get; }
    private IHttpClientFactory HttpClientFactory { get; }

    public HttpAuthApiService(ILogger logger, IHttpClientFactory httpClientFactory)
    {
        Logger = logger.ForContext<HttpAuthApiService>();
        HttpClientFactory = httpClientFactory;
    }

    public async Task Login(
        LoginRequest request,
        Func<HttpResponseMessage, LoginResponseOk, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.PostAsync(
            "/api/v1/auth/login",
            new StringContent(
                JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.LoginRequest),
                Encoding.UTF8,
                "application/json"
            )
        );

        if (res.IsSuccessStatusCode)
        {
            if (onOk is null)
                return;
            var loginResponse = JsonSerializer.Deserialize<LoginResponseOk>(
                await res.Content.ReadAsStringAsync(),
                AppJsonSerializerContext.Default.LoginResponseOk
            );
            ArgumentNullException.ThrowIfNull(loginResponse);
            await onOk(res, loginResponse);
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

    public async Task Logout(
        RegisterRequest request,
        Func<HttpResponseMessage, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.PostAsync(
            "/api/v1/auth/logout",
            new StringContent(
                JsonSerializer.Serialize(request, AppJsonSerializerContext.Default.LoginRequest),
                Encoding.UTF8,
                "application/json"
            )
        );
        if (res.IsSuccessStatusCode)
        {
            if (onOk is null)
                return;
            // var loginResponse = JsonSerializer.Deserialize<LoginResponseOk>(
            //     await res.Content.ReadAsStringAsync(),
            //     AppJsonSerializerContext.Default.LoginResponseOk
            // );
            // ArgumentNullException.ThrowIfNull(loginResponse);
            // await onOk(res, loginResponse);
            await onOk(res);
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

    public async Task Check(
        Func<HttpResponseMessage, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.GetAsync("/api/v1/auth/check");
        if (res.IsSuccessStatusCode)
        {
            if (onOk is null)
                return;
            await onOk(res);
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