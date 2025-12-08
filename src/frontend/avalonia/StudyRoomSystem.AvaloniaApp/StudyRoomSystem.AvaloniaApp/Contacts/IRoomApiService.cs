using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.AvaloniaApp.Contacts;

public interface IRoomApiService
{
    Task GetAll(
        Func<HttpResponseMessage, Room[], Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task GetBy(
        Guid id,
        Func<HttpResponseMessage, Room, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task Create();
}