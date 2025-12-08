using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.AvaloniaApp.Contacts;

public interface IBookingApiService
{
    Task GetMy(
        Func<HttpResponseMessage, Booking[], Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task GetBy(
        Guid id,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task Cancel(
        Guid id,
        Func<HttpResponseMessage, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task Create(
        CreateBookingRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task Edit(
        EditBookingRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task CheckIn(
        CheckInRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);

    Task CheckOut(
        CheckOutRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null);
}