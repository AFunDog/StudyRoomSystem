namespace StudyRoomSystem.Core.Structs.Exceptions;

public sealed class ForbidException(string message) : Exception(message);
