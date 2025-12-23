using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Api.V1;

namespace StudyRoomSystem.AvaloniaApp.Contacts;

public interface IAuthApiService
{
    public Task Login(
        LoginRequest request,
        Func<HttpResponseMessage, LoginResponseOk, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task Logout(
        Func<HttpResponseMessage, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task Check(
        Func<HttpResponseMessage, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);
}