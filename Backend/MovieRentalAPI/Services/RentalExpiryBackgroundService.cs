using MovieRentalModels;
using Microsoft.EntityFrameworkCore;

namespace MovieRentalAPI.Services
{
    /// <summary>
    /// Runs every 30 minutes to push "expiring soon" (24h window) and
    /// "just expired" notifications to users.
    /// </summary>
    public class RentalExpiryBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RentalExpiryBackgroundService> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);

        public RentalExpiryBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<RentalExpiryBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RentalExpiryBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var notifService = scope.ServiceProvider.GetRequiredService<NotificationService>();

                    await notifService.CheckExpiringRentals();
                    await notifService.CheckExpiredRentals();

                    _logger.LogInformation("Rental expiry checks completed at {Time}.", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during rental expiry check.");
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}
