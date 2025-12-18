using StudyRoomSystem.Server.Contacts;
using StudyRoomSystem.Server.Services.HostedService;

namespace StudyRoomSystem.Server.Services;

public static class ServiceExtension
{
    public static IServiceCollection AddInternalServices(this IServiceCollection serviceCollection)
    {
        // 添加 HostedService
        serviceCollection.AddHostedService<PgSqlNotificationsService>().AddHostedService<UpdateDatabaseService>();

        serviceCollection.AddTransient<IBlacklistService, BlacklistService>();
        return serviceCollection;
    }
}