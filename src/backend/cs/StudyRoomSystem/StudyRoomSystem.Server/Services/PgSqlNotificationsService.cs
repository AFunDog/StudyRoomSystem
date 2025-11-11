using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using Serilog;
using StudyRoomSystem.Core.Structs;
using StudyRoomSystem.Server.Database;
using StudyRoomSystem.Server.Hubs;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.Server.Services;

public class PgSqlNotificationsService : IHostedService
{
    private IConfiguration Configuration { get; }
    private IHubContext<DataHub> DataHub { get; }
    private IServiceScopeFactory ServiceScopeFactory { get; }
    private IOptions<JsonOptions> JsonOptions { get; }
    private NpgsqlConnection? Connection { get; set; }
    private CancellationTokenSource BackgroundTaskCts { get; } = new();
    private Task? BackgroundTask { get; set; }

    public PgSqlNotificationsService(
        IConfiguration configuration,
        IHubContext<DataHub> dataHub,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<JsonOptions> jsonOptions)
    {
        Configuration = configuration;
        DataHub = dataHub;
        ServiceScopeFactory = serviceScopeFactory;
        JsonOptions = jsonOptions;
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            Connection = new NpgsqlConnection(Configuration.GetConnectionString("DefaultConnection"));
            await Connection.OpenAsync(cancellationToken);

            Connection.Notification += OnNotification;

            await using var cmd = Connection.CreateCommand();
            cmd.CommandText = "LISTEN data_change";
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            BackgroundTask = Task.Run(
                async () =>
                {
                    try
                    {
                        while (!BackgroundTaskCts.IsCancellationRequested)
                        {
                            await Connection.WaitAsync(BackgroundTaskCts.Token);
                        }
                    }
                    catch (OperationCanceledException e)
                    {
                        Log.Logger.Trace().Information(e, "任务取消");
                    }
                    catch (Exception e)
                    {
                        Log.Logger.Trace().Error(e, "");
                    }
                },
                cancellationToken
            );

            Log.Logger.Trace().Information("启动");
        }
        catch (Exception e)
        {
            Log.Logger.Trace().Error(e, "启动");
        }
    }

    class PayloadData
    {
        public string Table { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public Guid DataId { get; set; }
    }

    private void OnNotification(object sender, NpgsqlNotificationEventArgs e)
    {
        Log.Logger.Trace().Information("NOTIFY {@Args}", e);
        using var scope = ServiceScopeFactory.CreateAsyncScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var payloadData = JsonSerializer.Deserialize<PayloadData>(
            e.Payload,
            options: JsonOptions.Value.SerializerOptions
        );
        if (payloadData is null)
            return;
        // TODO 消息队列处理任务 Channel
        if (payloadData.Table == "bookings")
        {
            // TODO 根据表名通知前台数据更新
            // DataHub.Clients.All.SendAsync("bookings-update", payloadData.DataId);
            var booking = appDbContext.Bookings.Find(payloadData.DataId);
            if (booking is null)
                return;
            DataHub
                .Clients.User(booking.UserId.ToString())
                .SendAsync(
                    "bookings-my-update",
                    appDbContext
                        .Bookings.Include(x => x.Seat)
                        .Include(x => x.Seat.Room)
                        .Where(x => x.UserId == booking.UserId)
                        .ToList()
                );
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await BackgroundTaskCts.CancelAsync();
        BackgroundTaskCts.Dispose();
        if (BackgroundTask is not null)
            await BackgroundTask;

        if (Connection is not null)
        {
            Connection.Notification -= OnNotification;
            await Connection.CloseAsync();
        }

        Log.Logger.Trace().Information("停止");
    }
}