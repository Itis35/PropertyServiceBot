using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

class Program
{
    private static ITelegramBotClient _bot;
    private static string _specialistChatId = "-4787188492";

    static async Task Main(string[] args)
    {
        _bot = new TelegramBotClient("7407315720:AAF3anRLktDY7q3ygRbcUSRIGWdQfc2bAKI");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _bot.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, receiverOptions);

        Console.WriteLine("Бот запущен. Нажмите Ctrl+C для остановки...");
        await Task.Delay(-1);
    }

    private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            // Обработка текстовых сообщений
            if (update.Message is { } message)
            {
                var chatId = message.Chat.Id;
                var username = message.From?.Username ?? "неизвестный пользователь";

                // Игнорируем команды типа /start
                if (message.Text?.StartsWith("/") == true)
                {
                    await bot.SendTextMessageAsync(
                        chatId,
                        "Просто напишите описание вашей проблемы, и мы сразу её обработаем! ✍️\n\nК описанию можно прикрепить фото/документ",
                        cancellationToken: ct
                    );
                    return;
                }

                // Если есть текст или медиа
                if (!string.IsNullOrEmpty(message.Text) || message.Photo != null || message.Document != null)
                {
                    await ProcessProblemRequest(bot, message, ct);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка обработки: {ex.Message}");
        }
    }

    private static async Task ProcessProblemRequest(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var username = message.From?.Username ?? "неизвестный пользователь";
        var userId = message.From?.Id;

        // Формируем текст заявки
        var description = message.Text ?? message.Caption ?? "без описания";
        var requestText = $"🚨 *Новая заявка*\n\n" +
                         $"📝 *Описание*: {description}\n" +
                         $"👤 *Пользователь*: @{username}\n";
                         //$"🆔 *ID чата*: `{chatId}`";

        // Кнопка для связи
        var contactButton = InlineKeyboardButton.WithUrl(
            "✉ Написать пользователю",
            $"tg://user?id={userId}"
        );

        // Если есть фото
        if (message.Photo is { } photo)
        {
            await bot.SendPhotoAsync(
                _specialistChatId,
                new InputFileId(photo.Last().FileId),
                caption: requestText,
                parseMode: ParseMode.Markdown,
                replyMarkup: new InlineKeyboardMarkup(contactButton),
                cancellationToken: ct
            );
        }
        // Если есть документ
        else if (message.Document is { } document)
        {
            await bot.SendDocumentAsync(
                _specialistChatId,
                new InputFileId(document.FileId),
                caption: requestText,
                parseMode: ParseMode.Markdown,
                replyMarkup: new InlineKeyboardMarkup(contactButton),
                cancellationToken: ct
            );
        }
        // Если только текст
        else
        {
            await bot.SendTextMessageAsync(
                _specialistChatId,
                requestText,
                parseMode: ParseMode.Markdown,
                replyMarkup: new InlineKeyboardMarkup(contactButton),
                cancellationToken: ct
            );
        }

        // Подтверждение пользователю с кнопкой для новой заявки
        var replyKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton("/start")
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await bot.SendTextMessageAsync(
            chatId,
            "✅ Ваша заявка отправлена специалистам!\n" +
            @"Если нужно сообщить о другой проблеме - пришлите команду /start",
            replyMarkup: replyKeyboard,
            cancellationToken: ct
        );
    }

    private static Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        Console.WriteLine($"Ошибка: {exception.Message}");
        return Task.CompletedTask;
    }
}