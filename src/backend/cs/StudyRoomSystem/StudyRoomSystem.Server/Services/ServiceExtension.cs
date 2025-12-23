using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Services.HostedService;

namespace StudyRoomSystem.Server.Services;

public static class ServiceExtension
{
    public static IServiceCollection AddInternalServices(this IServiceCollection serviceCollection)
    {
        // 异常处理
        serviceCollection.AddProblemDetails();
        serviceCollection.AddExceptionHandler<GlobalExceptionHandler>();

        // 添加 HostedService
        serviceCollection.AddHostedService<PgSqlNotificationsService>().AddHostedService<UpdateDatabaseService>();

        serviceCollection
            .AddTransient<IComplaintService, ComplaintService>()
            .AddTransient<IViolationService, ViolationService>()
            .AddTransient<IBookingService, BookingService>()
            .AddTransient<IRoomService, RoomService>()
            .AddTransient<IBlacklistService, BlacklistService>()
            .AddTransient<IUserService, UserService>();
        return serviceCollection;
    }
}