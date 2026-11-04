using System.Text.RegularExpressions;
using Npgsql.Replication;
using Telegram.Bot;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YarnBot.Data;
using YarnBot.Dto;
using YarnBot.Models;
using YarnBot.Repo;

namespace YarnBot.Services;

public class Handler(ITelegramBotClient client, IApiService apiService, ILogger<Handler> logger, MessagesService messagesService, UserRepo repo)
{
    private static readonly Regex NonDigitRegex = new(@"\D", RegexOptions.Compiled);
    public async Task HandleMessage(Message message, CancellationToken stoppingToken)
    {
        var text = (message.Text ?? string.Empty).Trim().ToLower();
        var chatId = message.Chat.Id;
        bool hasActivePromo = false;

        var user = await repo.FindUserByChatId(chatId, stoppingToken);

        if (user != null)
        {
            if (user.UpdatedAt.AddHours(1) <= DateTime.UtcNow)
            {
                try
                {
                    await SyncPromocodes(user, stoppingToken);
                }
                catch (InvalidOperationException ex)
                {
                    await client.SendPhoto(chatId, messagesService.noComandPhoto, messagesService.errorMessage, ParseMode.Html, cancellationToken: stoppingToken);
                    logger.LogWarning(ex, "Promocodes weren't updated for user- {ClientId}", user.ClientId);
                    return;
                }
            }

            hasActivePromo = user.Promocodes.Any();
        }

        if (text.StartsWith("/start"))
        {
            if (user is null)
            {
                await client.SendPhoto(chatId, messagesService.catPhonePhoto, messagesService.askingNumberText, ParseMode.Html, replyMarkup: messagesService.askContact, cancellationToken: stoppingToken);
            }
            else
            {
                if (hasActivePromo)
                {
                    var messageToSend = messagesService.FormPromoMessage(user.Promocodes);
                    await client.SendPhoto(chatId, messagesService.promoPhoto, messageToSend, ParseMode.Html, replyMarkup: messagesService.promoInlineMarkup, cancellationToken: stoppingToken);
                }
                else
                {
                    await client.SendPhoto(chatId, messagesService.promoPhoto, messagesService.noPromoMessage, ParseMode.Html, replyMarkup: messagesService.NoPromoInlineMarkup, cancellationToken: stoppingToken);
                }
            }

            await repo.SaveChangesAsync(stoppingToken);
            return;
        }

        if (message.Contact is not null && message.Contact.UserId == message.Chat.Id)
        {
            if (user is null)
            {
                var phoneNumber = NonDigitRegex.Replace(message.Contact.PhoneNumber, "");

                var firstMessage = await client.SendMessage(chatId, messagesService.waitingText, ParseMode.Html, replyMarkup: new ReplyKeyboardRemove(), cancellationToken: stoppingToken);
                ClientDto? userOneC;

                try
                {
                    userOneC = await apiService.GetClientByPhoneAsync(phoneNumber, stoppingToken);
                }
                catch(HttpRequestException ex)
                {
                    if(ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        await client.DeleteMessage(chatId, firstMessage.MessageId, cancellationToken: stoppingToken);
                        await client.SendPhoto(chatId, messagesService.noComandPhoto, messagesService.duplicatePhoneMessage, ParseMode.Html, replyMarkup: messagesService.duplicatePhoneMarkup, cancellationToken: stoppingToken);
                        return;
                    }
                    else
                    {
                        throw;
                    }    
                }

                await client.DeleteMessage(chatId, firstMessage.MessageId, cancellationToken: stoppingToken);

                if (userOneC is null)
                {
                    await client.SendPhoto(chatId, messagesService.noComandPhoto, messagesService.duplicatePhoneMessage, ParseMode.Html, replyMarkup: messagesService.duplicatePhoneMarkup, cancellationToken: stoppingToken);
                    logger.LogWarning("Authorisation failed for customer with {phoneNumber} in chatId- {chatId}", phoneNumber, chatId);
                    return;
                }

                if (userOneC.Promocodes.Count > 0)
                {
                    string messageToSend = messagesService.FormPromoMessage(UserService.CreatePromos(userOneC.Promocodes));
                    await client.SendPhoto(chatId, messagesService.promoPhoto, messageToSend, ParseMode.Html, replyMarkup: messagesService.promoInlineMarkup, cancellationToken: stoppingToken);
                }
                else
                {
                    await client.SendPhoto(chatId, messagesService.promoPhoto, messagesService.noPromoMessage, ParseMode.Html, replyMarkup: messagesService.NoPromoInlineMarkup, cancellationToken: stoppingToken);
                }

                await repo.AddAsync(UserService.CreateUser(userOneC, chatId), stoppingToken);
            }
            else
            {
                if (hasActivePromo)
                {
                    var messageToSend = messagesService.FormPromoMessage(user.Promocodes);
                    await client.SendPhoto(chatId, messagesService.promoPhoto, messageToSend, ParseMode.Html, replyMarkup: messagesService.promoInlineMarkup, cancellationToken: stoppingToken);
                }
                else
                {
                    await client.SendPhoto(chatId, messagesService.promoPhoto, messagesService.noPromoMessage, ParseMode.Html, replyMarkup: messagesService.NoPromoInlineMarkup, cancellationToken: stoppingToken);
                }
            }
            await repo.SaveChangesAsync(stoppingToken);
            return;
        }

        if (user is null)
        {
            await client.SendPhoto(chatId, messagesService.catPhonePhoto, messagesService.noNumberText, ParseMode.Html, replyMarkup: messagesService.askContact, cancellationToken: stoppingToken);
            return;
        }
        else
        {
            await client.SendPhoto(chatId, messagesService.noComandPhoto, messagesService.noContextText, ParseMode.Html, replyMarkup: messagesService.noContextMarkup, cancellationToken: stoppingToken);
        }
    }
    public async Task HandleQueryAsync(CallbackQuery query, CancellationToken stoppingToken)
    {
        if (query.Message is null || query.Data is null)
        {
            return;
        }

        var chatId = query.Message.Chat.Id;
        var data = query.Data;
        bool hasActivePromo = false;

        var user = await repo.FindUserByChatId(chatId, stoppingToken);

        if (user != null)
        {
            if (user.UpdatedAt.AddHours(1) <= DateTime.UtcNow)
            {
                try
                {
                    await SyncPromocodes(user, stoppingToken);
                }
                catch (InvalidOperationException)
                {
                    await client.SendPhoto(chatId, messagesService.noComandPhoto, messagesService.errorMessage, ParseMode.Html, cancellationToken: stoppingToken);
                    return;
                }
            }

            hasActivePromo = user.Promocodes.Any();
        }

        if (user != null && data == "Return_to_promo")
        {
            await client.AnswerCallbackQuery(query.Id, $"Повертаємось до промо... ✨", cancellationToken: stoppingToken);

            if (hasActivePromo)
            {
                var messageToSend = messagesService.FormPromoMessage(user.Promocodes);
                await client.SendPhoto(chatId, messagesService.promoPhoto, messageToSend, ParseMode.Html, replyMarkup: messagesService.promoInlineMarkup, cancellationToken: stoppingToken);
            }
            else
            {
                await client.SendPhoto(chatId, messagesService.promoPhoto, messagesService.noPromoMessage, ParseMode.Html, replyMarkup: messagesService.NoPromoInlineMarkup, cancellationToken: stoppingToken);
            }
            await repo.SaveChangesAsync(stoppingToken);
            return;
        }
    }
    public async Task SyncPromocodes(Models.User user, CancellationToken stoppingToken)
    {
        var userOneC = await apiService.GetClientByIdAsync(user.ClientId, stoppingToken);
        if (userOneC == null)
        {
            logger.LogWarning("User with {ClientId} wasn't found", user.ClientId);
            throw new InvalidOperationException($"User with {user.ClientId} wasn't found");
        }
        var promosToRemove = user.SyncPromocodes(UserService.CreatePromos(userOneC.Promocodes));
        repo.RemovePromocodes(promosToRemove);
        user.UpdatedAt = DateTime.UtcNow;
    }
}