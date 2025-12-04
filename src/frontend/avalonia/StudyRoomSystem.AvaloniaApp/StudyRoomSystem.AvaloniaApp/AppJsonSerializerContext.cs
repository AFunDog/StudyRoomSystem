using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using StudyRoomSystem.AvaloniaApp.ViewModels;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Core.Structs.Api;
using StudyRoomSystem.Core.Structs.Entity;

namespace StudyRoomSystem.AvaloniaApp;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]

#region Api

[JsonSerializable(typeof(ProblemDetails))]

[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(LoginResponseOk))]

[JsonSerializable(typeof(RegisterRequest))]

#endregion

#region Entity

[JsonSerializable(typeof(Block))]
[JsonSerializable(typeof(Booking))]
[JsonSerializable(typeof(Booking[]))]
[JsonSerializable(typeof(Complaint))]
[JsonSerializable(typeof(Room))]
[JsonSerializable(typeof(Room[]))]
[JsonSerializable(typeof(Seat))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(Violation))]

#endregion

internal sealed partial class AppJsonSerializerContext : JsonSerializerContext { }