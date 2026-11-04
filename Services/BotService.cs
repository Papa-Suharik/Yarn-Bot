using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YarnBot.Services;

namespace YarnBot.Services;

public class BotService(ILogger<BotService> loggger, IServiceScopeFactory scopeFactory, ITelegramBotClient client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var me = await client.GetMe(stoppingToken);
        loggger.LogInformation("Bot @{Username} has been succsessfuly launched in container!", me.Username);

        client.StartReceiving(
            updateHandler: HandleUpdate,
            errorHandler: HandleError,
            cancellationToken: stoppingToken
        );

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    public async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var messageHandler = scope.ServiceProvider.GetRequiredService<Handler>();

            if (update.Type is UpdateType.Message)
            {
                if (update.Message is null || update.Message.From is null)
                {
                    return;
                }
                await messageHandler.HandleMessage(update.Message, stoppingToken);
            }
            else if (update.Type is UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery is null || update.CallbackQuery.From is null)
                {
                    return;
                }
                await messageHandler.HandleQueryAsync(update.CallbackQuery, stoppingToken);
            }
        }
        catch(Exception ex)
        {
            loggger.LogError(ex, "HandleUpdate error occured with message- {Message}", ex.Message);
        }
    }
    public async Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken stoppingToken)
    {
        loggger.LogWarning(exception, "Telegram recieving error");
    }
}