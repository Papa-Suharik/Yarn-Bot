using Microsoft.VisualBasic;
using Telegram.Bot.Types.ReplyMarkups;
using YarnBot.Dto;
using YarnBot.Models;

namespace YarnBot.Services;

public class MessagesService
{
    public string catPhonePhoto = "AgACAgIAAxkDAAICfmrGiKHi4XuHjEoc_yA-T_-hNfAZAAJeIGsbE34xSi-R80WdjfOjAQADAgADeAADPQQ";
    public string promoPhoto = "AgACAgIAAxkDAAPpaqgIih0e5xp8pceMeHsnpP7SNDUAAn8kaxubYEFJjHOlFETtLfsBAAMCAAN5AAM9BA";
    public string reminderPhoto = "AgACAgIAAxkDAAIBj2q1Ol_GjqdONyTqPILbqoZU7lt3AAJHIWsbEiCoSfS2BKuGs75KAQADAgADeAADPQQ";
    public string noUserPhoto = "AgACAgIAAxkDAAIBjWq1OkDih-8MFxhh7_EHSNwAAZrFDwACRSFrGxIgqEmSS-Nprc_9XgEAAwIAA3gAAz0E";
    public string noComandPhoto = "AgACAgIAAxkDAAICj2rHpCxQMN20Xh5RU0mjuvM6qYQuAAK1HWsbE345StWw3l0CNO1yAQADAgADeAADPQQ";
    public string duplicatePhoneMessage = """
    <b>Не вдалося знайти ваш акаунт за цим номером телефону<tg-emoji emoji-id="5435857806936777022">🔥</tg-emoji>

    Можливо, сталася помилка під час пошуку. Давайте допоможемо вам розібратися — зв’яжіться з нашим менеджером<tg-emoji emoji-id="5215334566549540768">🔥</tg-emoji></b>
    """;
    public string errorMessage = """
    <b>"Не вдалося оновити дані. Спробуйте пізніше."</b>
    """;
    public string noNumberText = $"""
    Ой, здається, щось пішло не так👀 Ми не отримали ваш номер телефону.
    Поділіться ним, будь ласка, натиснувши кнопку нижче — і ми покажемо ваші промокоди 🩷
    """;
    public string noContextText = $"""
    Якщо у вас виникли питання або потрібна консультація — натисніть кнопку нижче!
    """;
    public string noUserOneCText = $"""
    <b>Схоже, ми ще не знайомі!<tg-emoji emoji-id="6122730271360946438">🔥</tg-emoji>

    За цим номером ми ще не знайшли покупок. Тож давайте знайомитися<tg-emoji emoji-id="5379732256644405206">🔥</tg-emoji>

    Обирайте все необхідне для творчості тут:</b>
    """;
    public string askingNumberText = $"""
    <tg-emoji emoji-id="5854890673353789054">🔥</tg-emoji><b>Привіт, я бот-помічник від Candy-Yarn!</b>

    <tg-emoji emoji-id="5960892974479969978">🔥</tg-emoji><b>Що ти тут знайдеш:</b> 
    -Персональні знижки та промокоди
    -Безкоштовний майстер-клас
    -Доступ до спільноти майстрів та майстринь

    <tg-emoji emoji-id="5262831879731555779">🔥</tg-emoji>Щоб побачити персональні знижки поділіться номером телефону
    """;
    public string waitingText = """
    Пакуємо ваші подарунки<tg-emoji emoji-id="5316880439674155768">🔥</tg-emoji>
    """; 
    public string promoMessageTop = $"""
    <b><tg-emoji emoji-id="5341734105451085914">🔥</tg-emoji>Дякуємо що обираєте Candy-Yarn!</b>

    <b><tg-emoji emoji-id="5305358683096494349">🔥</tg-emoji>Промокоди на наступні покупки:</b>
    
    """;
    public string promoMessageBot = $"""
    <b>
    Натисніть на промокод, щоб скопіювати його, а використати його можна на нашому сайті

    <tg-emoji emoji-id="5442939099906325301">🔥</tg-emoji>Посилання на сайт та інші подарунки:
    </b>
    """;
    public string noPromoMessage = $"""
    <b><tg-emoji emoji-id="5341734105451085914">🔥</tg-emoji>Дякуємо що обираєте Candy-Yarn!

    У вас наразі немає активних промокодів<tg-emoji emoji-id="5418133868475587618">🔥</tg-emoji>

    Але це не біда — поки що запрошуємо зазирнути на наш сайт або приєднатися до нашої спільноти)

    Там завжди знайдеться щось цікаве<tg-emoji emoji-id="5409262403072306944">🔥</tg-emoji></b>
    """;
    public readonly InlineKeyboardMarkup duplicatePhoneMarkup = new(
        [
            [new InlineKeyboardButton("Звʼязатися з менеджером", "https://t.me/candy_yarn"){
                IconCustomEmojiId = "5235826817591420001"
            }]
        ]
    );
    public readonly InlineKeyboardMarkup noContextMarkup = new(
        [
            [new InlineKeyboardButton("Звʼязатися з менеджером", "https://t.me/candy_yarn"){
                IconCustomEmojiId = "5235826817591420001"
            }],
            [new InlineKeyboardButton("Повернутися на сторінку з промо"){
                CallbackData = "Return_to_promo",
                IconCustomEmojiId = "5305358683096494349"
            }]
        ]
    );
    public readonly InlineKeyboardMarkup followupMarkup = new (
        [
            [new InlineKeyboardButton("Використати промокод", "https://candy-yarn.ua/?utm_source=telegram&utm_medium=bot"){
                IconCustomEmojiId = "5199874732983353088"
            }],
            [new InlineKeyboardButton("Повернутися на сторінку з промо"){
                CallbackData = "Return_to_promo",
                IconCustomEmojiId = "5305358683096494349"
            }]
        ]
    );

    public readonly InlineKeyboardMarkup NoPromoInlineMarkup = new (
        [
            [new InlineKeyboardButton("Переходь на наш сайт", "https://candy-yarn.ua/?utm_source=telegram&utm_medium=bot"){
                IconCustomEmojiId = "5199874732983353088"
            }],
            [new InlineKeyboardButton("Спільнота майстринь", "https://t.me/+RZ8CXV2d739iYmIy"){
                IconCustomEmojiId = "5330237710655306682"
            }],
            [new InlineKeyboardButton("Майстер-класс у подарунок", "https://drive.google.com/file/d/10KATiR9x2ISPJBlvw_VbnVCd68YSywP5/view?usp=sharing"){
                IconCustomEmojiId = "5197498864449371267"
            }]
        ]
    );
    public readonly InlineKeyboardMarkup promoInlineMarkup = new (
        [
            [new InlineKeyboardButton("Використати промокод", "https://candy-yarn.ua/?utm_source=telegram&utm_medium=bot"){
                IconCustomEmojiId = "5199874732983353088"
            }],
            [new InlineKeyboardButton("Майстер-класс у подарунок", "https://drive.google.com/file/d/10KATiR9x2ISPJBlvw_VbnVCd68YSywP5/view?usp=sharing"){
                IconCustomEmojiId = "5197498864449371267"
            }],
            [new InlineKeyboardButton("Спільнота майстринь", "https://t.me/+RZ8CXV2d739iYmIy"){
                IconCustomEmojiId = "5330237710655306682"
            }]
        ]
    );
    public readonly InlineKeyboardMarkup noUserOneCMarkup = new(
        [
            [new InlineKeyboardButton("Наш сайт", "https://candy-yarn.ua/?utm_source=telegram&utm_medium=bot"){
                IconCustomEmojiId = "5199874732983353088"
            }],
            [new InlineKeyboardButton("Спільнота майстринь", "https://t.me/+RZ8CXV2d739iYmIy"){
                IconCustomEmojiId = "5330237710655306682"
            }]
        ]
    );
    public readonly KeyboardButton askContact = new("➡️ВЕРИФІКАЦІЯ ЗА НОМЕРОМ ТЕЛЕФОНУ⬅️", requestContact: true);
    public string FormPromoMessage(List<Promocode> list)
    {
        string promos = FormPromocodes(list);
        return promoMessageTop + promos + promoMessageBot;
    }
    public string FormPromocodes(List<Promocode> list)
    {
        var kyivTz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        string codes = "";
        foreach(Promocode promocode in list)
        {
            var expiresAt = TimeZoneInfo.ConvertTimeFromUtc(promocode.ExpiresAt, kyivTz); 
            var timeLeft = promocode.ExpiresAt - DateTime.UtcNow;
            codes += $"""
            <b>
            <blockquote>Промокод: <code>{promocode.Code}</code>
            {promocode.Description}
            Діє до: {expiresAt:dd.MM.yyyy}
            Залишилось: {timeLeft.Days} дн. {timeLeft.Hours} год.
            </blockquote>
            </b>
            """;
        }
        return codes;
    }
}