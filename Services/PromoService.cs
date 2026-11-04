using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using YarnBot.Dto;
using YarnBot.Repo;
using System.Linq.Expressions;
using System.Globalization;
using Telegram.Bot.Exceptions;

namespace YarnBot.Services;

public class PromoService(IApiService apiService, UserRepo repo, ITelegramBotClient client, MessagesService messagesService, IConfiguration configuration, ILogger<PromoService> logger)
{
    public async Task FollowupCustomers(CancellationToken cancellationToken)
    {
        long adminChatId = configuration.GetValue<long>("Telegram:AdminChatId");
        int followupCount;
        int followupCompleted = 0;
        int followupFailed = 0;

        var idsToFollowUp = await repo.GetPromoIdsToFollowupAsync(cancellationToken);

        if (idsToFollowUp.Count == 0)
        {
            await client.SendMessage(adminChatId, "There are no customers to followup found in the bot database", cancellationToken: cancellationToken);
            logger.LogInformation("There are no customers to followup in bot database");
            return;
        }

        var requestBody = new ExpiringPromoRequestDto(idsToFollowUp);

        var expiringPromocodes = await apiService.GetExpiringPromosAsync(requestBody, cancellationToken) ?? throw new Exception("Requested resource is not found on the 1C server");

        if (expiringPromocodes.Promos.Count == 0)
        {
            await client.SendMessage(adminChatId, "1C returned 0 active promocodes to followup", cancellationToken: cancellationToken);
            logger.LogInformation("1C returned 0 active promocodes to followup");
            return;
        }

        followupCount = expiringPromocodes.Promos.Count;

        foreach (var promocode in expiringPromocodes.Promos)
        {
            try
            {
                await SendFollowupMessage(promocode, cancellationToken);
                followupCompleted++;
                await repo.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Reminder sent to {ClientId} about promo {Promocode}", promocode.ClientId, promocode.PromoCode);
            }
            catch(ApiRequestException ex) when (ex.ErrorCode == 429)
            {
                var retryAfter = ex.Parameters?.RetryAfter ?? 5;

                logger.LogWarning(ex, "Telegram rate limit exceeded. Retrying after {RetryAfter}", retryAfter);
                await Task.Delay(TimeSpan.FromSeconds(retryAfter), cancellationToken);
                
                await SendFollowupMessage(promocode, cancellationToken);
                followupCompleted++;
                await repo.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Reminder sent to {ClientId} about promo {Promocode}", promocode.ClientId, promocode.PromoCode);

            }
            catch(ApiRequestException ex) when (ex.ErrorCode == 403 || ex.ErrorCode == 400)
            {
                followupFailed ++;
                logger.LogWarning(ex, "Followup failed for customer with id: {CientId} since Telegram returned ApiRequestExpection- {Message}", promocode.ClientId, ex.Message);
            }
            catch (Exception ex)
            {
                followupFailed++;
                logger.LogWarning(ex, "Followup failed for customer with id: {ClientId} promo: {Promocode}", promocode.ClientId, promocode.PromoCode);
            }
        }

        string followupResults = $"""
        Results of the followup:
        Clients scheduled to followup- {followupCount}
        Sucsessfuly followedup- {followupCompleted}
        Failed to followup- {followupFailed} 
        """;
        await client.SendMessage(adminChatId, followupResults, cancellationToken: cancellationToken);
        logger.LogInformation("Followup finished. Scheduled:{Scheduled} Success:{Success} Failed:{Failed}", followupCount, followupCompleted, followupFailed);
    }
    public async Task SendFollowupMessage(ExpiringPromocodeDto dto, CancellationToken cancellationToken)
    {
        var user = await repo.FindUserByClientId(dto.ClientId, cancellationToken) ?? throw new Exception($"There is no customer under id- {dto.ClientId}");

        var userPromo = user.Promocodes.FirstOrDefault(x => x.PromoId == dto.IssuedPromoId) ?? throw new Exception($"Missmatch on the promos to followup. Promo to follow - {dto.PromoCode} UserId - {user.ClientId}");

        var kyivTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        var currentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, kyivTz);
        var expirationDate = TimeZoneInfo.ConvertTimeFromUtc(userPromo.ExpiresAt, kyivTz);
        var timeLeft = expirationDate - currentTime;

        string message = $"""
        <b>Нагадуємо що у вас залишився невикористаний промокод!
        
        <blockquote>Промокод: {userPromo.Code}
        {userPromo.Description}
        Діє до: {expirationDate:dd.MM.yyyy}
        Залишилось: {timeLeft.Days} дн. {timeLeft.Hours} год. {timeLeft.Minutes} хв.
        </blockquote>

        <tg-emoji emoji-id="5888758586426789596">🔥</tg-emoji>Щоб скористатися знижкою, переходьте на наш сайт — просто натисніть кнопку нижче!</b>
        """;

        await client.SendPhoto(user.ChatId, messagesService.reminderPhoto, message, ParseMode.Html, replyMarkup: messagesService.followupMarkup, cancellationToken: cancellationToken);
        userPromo.Followuped = true;
    }
}