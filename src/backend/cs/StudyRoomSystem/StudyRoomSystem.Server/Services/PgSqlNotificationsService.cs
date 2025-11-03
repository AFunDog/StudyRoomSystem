using Microsoft.EntityFrameworkCore;
using StudyRoomSystem.Server.Database;

namespace StudyRoomSystem.Server.Services;

public class PgSqlNotificationsService : IHostedService
{
    private AppDbContext AppDbContext { get; }
    
    
    public PgSqlNotificationsService(AppDbContext appDbContext)
    {
        AppDbContext = appDbContext;
    }
    
    
    public async Task StartAsync(CancellationToken cancellationToken) 
    {
        try { }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken) 
    {
        
    }
}
