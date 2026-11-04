namespace YarnBot.Services;

public class FollowupService(ILogger<FollowupService> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            var nextRun = now.Date.AddHours(7);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;

            logger.LogInformation("Next promo check at {nextRun}", nextRun);

            await Task.Delay(delay, stoppingToken);

            using var scope = scopeFactory.CreateScope();
            var notifyService = scope.ServiceProvider.GetRequiredService<AdminNotifierSerive>();
            var promoService = scope.ServiceProvider.GetRequiredService<PromoService>();
            
            try
            {
                await promoService.FollowupCustomers(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Followup process failed");
                try
                {
                    string information = $"Followup process failed!\nType: {ex.GetType().Name}\nMessage: {ex.Message}";
                    await notifyService.NotifyAdmin(information, stoppingToken);
                }
                catch (Exception notifyEx)
                {
                    logger.LogError(notifyEx, "Failed to notify Admin");
                }
            }
        }

    }
}