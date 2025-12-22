namespace StudyRoomSystem.Core.Structs.Exceptions;

public class UnauthorizedException(string? message = null) : Exception(message ?? "未授权或者未登录");
