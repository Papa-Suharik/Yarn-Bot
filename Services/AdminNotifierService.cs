using Telegram.Bot;

namespace YarnBot.Services;
public class AdminNotifierSerive(ITelegramBotClient client, ILogger<AdminNotifierSerive> logger, IConfiguration configuration)
{
    private readonly long AdminChatId = configuration.GetValue<long>("Telegram:AdminChatId");
    public async Task NotifyAdmin(string message, CancellationToken cancellationToken)
    {
        await client.SendMessage(AdminChatId, message, cancellationToken: cancellationToken);
        logger.LogInformation("Sending admin notification");
    }
}