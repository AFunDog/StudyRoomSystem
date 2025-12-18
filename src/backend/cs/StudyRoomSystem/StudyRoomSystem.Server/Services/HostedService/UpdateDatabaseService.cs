using Microsoft.EntityFrameworkCore;
using Serilog;
using StudyRoomSystem.Core.Structs.Entity;
using StudyRoomSystem.Server.Database;
using Zeng.CoreLibrary.Toolkit.Logging;

namespace StudyRoomSystem.Server.Services.HostedService;

public class UpdateDatabaseService(IServiceScopeFactory serviceScopeFactory) : IHostedService
{
    private IServiceScopeFactory ServiceScopeFactory { get; } = serviceScopeFactory;
    private Task? BackgroundTask { get; set; }
    private CancellationTokenSource BackgroundTaskCts { get; } = new();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        BackgroundTask = Task.WhenAll(Task.Run(BookingTimeout, cancellationToken));
    }

    private async Task BookingTimeout()
    {
        var token = BackgroundTaskCts.Token;
        while (!token.IsCancellationRequested)
        {
            await using var scope = ServiceScopeFactory.CreateAsyncScope();
            var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var res = 0;
            var 没签到预约 = appDbContext
                .Bookings.Where(x => x.StartTime.AddMinutes(15) < DateTime.UtcNow)
                .Where(x => x.State == BookingStateEnum.Booked);

            var 没签退预约 = appDbContext
                .Bookings.Where(x => x.EndTime.AddMinutes(15) < DateTime.UtcNow)
                .Where(x => x.State == BookingStateEnum.CheckIn);
            var 添加违规 = 没签到预约
                .ToList()
                .Select(x => new Violation()
                    {
                        Id = Ulid.NewUlid().ToGuid(),
                        UserId = x.UserId,
                        BookingId = x.Id,
                        Content = "未在规定时间签到",
                        CreateTime = DateTime.UtcNow,
                        State = ViolationStateEnum.Violation,
                        Type = ViolationTypeEnum.超时,
                    }
                );
            res += await 没签到预约.ExecuteUpdateAsync(
                x => x.SetProperty(b => b.State, BookingStateEnum.Canceled),
                cancellationToken: token
            );
            await appDbContext.Violations.AddRangeAsync(添加违规, token);
            res += await appDbContext.SaveChangesAsync(token);
            添加违规 = 没签退预约
                .ToList()
                .Select(x => new Violation()
                    {
                        Id = Ulid.NewUlid().ToGuid(),
                        UserId = x.UserId,
                        BookingId = x.Id,
                        Content = "未在规定时间签退",
                        CreateTime = DateTime.UtcNow,
                        State = ViolationStateEnum.Violation,
                        Type = ViolationTypeEnum.超时,
                    }
                );
            res += await 没签退预约.ExecuteUpdateAsync(
                x => x.SetProperty(b => b.State, BookingStateEnum.Canceled),
                cancellationToken: token
            );
            await appDbContext.Violations.AddRangeAsync(添加违规, token);
            res += await appDbContext.SaveChangesAsync(token);
            if (res != 0)
            {
                Log.Logger.Trace().Information("更新和添加了 {Count} 条数据", res);
            }

            await Task.Delay(TimeSpan.FromSeconds(15), token);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await BackgroundTaskCts.CancelAsync();
    }
}