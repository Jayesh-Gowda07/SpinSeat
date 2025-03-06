using Microsoft.EntityFrameworkCore;
using spinApp.Data;

namespace spinApp.Services
{
    public class DailyCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DailyCleanupService> _logger;

        public DailyCleanupService(IServiceProvider services, ILogger<DailyCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1); // Next midnight
                var delay = nextRun - now;

                _logger.LogInformation("Next cleanup scheduled for {NextRun}", nextRun);
                await Task.Delay(delay, stoppingToken);

                using var scope = _services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                try
                {
                    var cutoffDate = DateTime.UtcNow.Date.AddDays(-1);
                    var deleted = await dbContext.DailyNumbers
                        .Where(dn => dn.Date < cutoffDate)
                        .ExecuteDeleteAsync(stoppingToken);

                    _logger.LogInformation("Deleted {Count} old seat assignments", deleted);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during seat cleanup");
                }
            }
        }
    }
}
