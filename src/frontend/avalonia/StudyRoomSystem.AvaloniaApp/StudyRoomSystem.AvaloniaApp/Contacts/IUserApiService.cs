using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;

namespace StudyRoomSystem.AvaloniaApp.Contacts;

public interface IUserApiService
{
    Task Register(
        RegisterRequest request,
        Func<HttpResponseMessage, User, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);
}