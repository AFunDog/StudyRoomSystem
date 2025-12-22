namespace StudyRoomSystem.Core.Structs.Exceptions;

public sealed class NotFoundException(string message) : Exception(message);
