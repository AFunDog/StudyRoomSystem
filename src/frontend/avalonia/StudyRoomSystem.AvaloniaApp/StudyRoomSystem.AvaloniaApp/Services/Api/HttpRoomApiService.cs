using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StudyRoomSystem.AvaloniaApp.Contacts;
using StudyRoomSystem.Core.Structs;

namespace StudyRoomSystem.AvaloniaApp.Services.Api;

internal sealed partial class HttpRoomApiService : IRoomApiService
{
    private ILogger Logger { get; }
    private IHttpClientFactory HttpClientFactory { get; }

    public HttpRoomApiService(ILogger logger, IHttpClientFactory httpClientFactory)
    {
        Logger = logger.ForContext<HttpAuthApiService>();
        HttpClientFactory = httpClientFactory;
    }
    
    public async Task GetAll(
        Func<HttpResponseMessage, Room[], Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.GetAsync("/api/v1/room");
        if (res.IsSuccessStatusCode)
        {
            if (onOk is null)
                return;
            var rooms = JsonSerializer.Deserialize<Room[]>(
                await res.Content.ReadAsStringAsync(),
                AppJsonSerializerContext.Default.RoomArray
            );
            ArgumentNullException.ThrowIfNull(rooms);
            await onOk(res, rooms);
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

    public async Task GetBy(
        Guid id,
        Func<HttpResponseMessage, Room, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null) { }

    public async Task Create() { }
}