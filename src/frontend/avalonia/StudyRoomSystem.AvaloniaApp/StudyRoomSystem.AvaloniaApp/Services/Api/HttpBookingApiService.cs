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

internal sealed partial class HttpBookingApiService : IBookingApiService
{
    private ILogger Logger { get; }
    private IHttpClientFactory HttpClientFactory { get; }

    public HttpBookingApiService(ILogger logger, IHttpClientFactory httpClientFactory)
    {
        Logger = logger.ForContext<HttpAuthApiService>();
        HttpClientFactory = httpClientFactory;
    }

    public async Task GetMy(
        Func<HttpResponseMessage, Booking[], Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.GetAsync($"/api/v1/booking");
        if (res.IsSuccessStatusCode)
        {
            if (onOk is null)
                return;
            var bookings = JsonSerializer.Deserialize<Booking[]>(
                await res.Content.ReadAsStringAsync(),
                AppJsonSerializerContext.Default.BookingArray
            );
            ArgumentNullException.ThrowIfNull(bookings);
            await onOk(res, bookings);
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
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.GetAsync($"/api/v1/booking/{id}");
        if (res.IsSuccessStatusCode)
        {
            if (onOk is null)
                return;
            var booking = JsonSerializer.Deserialize<Booking>(
                await res.Content.ReadAsStringAsync(),
                AppJsonSerializerContext.Default.Booking
            );
            ArgumentNullException.ThrowIfNull(booking);
            await onOk(res, booking);
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

    public async Task Cancel(
        Guid id,
        Func<HttpResponseMessage, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null)
    {
        var client = HttpClientFactory.CreateClient("API");
        var res = await client.GetAsync($"/api/v1/booking/{id}");
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

    public async Task Create(
        CreateBookingRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null) { }

    public async Task Edit(
        EditBookingRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null) { }

    public async Task CheckIn(
        CheckInRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null) { }

    public async Task CheckOut(
        CheckOutRequest request,
        Func<HttpResponseMessage, Booking, Task>? onOk = null,
        Func<HttpResponseMessage, ProblemDetails, Task>? onError = null) { }
}