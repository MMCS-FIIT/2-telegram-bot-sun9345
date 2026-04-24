using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MusicDiaryBot
{
    internal class Program
    {
        static TelegramBotClient botClient = new TelegramBotClient("8444705053:AAHtL9xr7Fu3P-XvOx-lzx4REHh8g0sOBxM");

        static async Task Main(string[] args)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message }
            };

            botClient.StartReceiving( // получение сообщений
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions
            );

            var me = await botClient.GetMe();
            Console.WriteLine($"бот запущен: @{me.Username}");

            Console.ReadLine(); // чтобы не выключился
        }

        static async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct) //хендлинг событий
        {
            if (update.Message == null || update.Message.Text == null) return; // проверка на null в сообщении
            string text = update.Message.Text;

            var chatId = update.Message.Chat.Id; // айди для чата с пользователем
            Console.WriteLine($"Получено сообщение от {chatId}: {text}");

            switch (text)
            {
                case "/start":
                    await client.SendMessage(chatId,
                        "--- Добро пожаловать в Музыкальный дневник! ---\n\n" +
                        "Ты можешь добавить свои любимые треки в библиотеку, ставить им оценки и получать рекомендации.\n\n" +
                        "/help чтобы увидеть список команд.",
                        cancellationToken: ct);
                    break;

                case "/help":
                    await client.SendMessage(chatId,
                        "--- Список команд: ---\n\n" +
                        "/add — добавить трек в библиотеку\n" +
                        "/list — посмотреть свою библиотеку\n" +
                        "/stats — статистика по библиотеке\n" +
                        "/recommend — получить рекомендации\n" +
                        "/help — список команд",
                        cancellationToken: ct);
                    break;

                default:
                    await client.SendMessage(chatId,
                        "Нет такой команды:(\n\n" +
                        "Напиши /help чтобы увидеть список доступных команд.",
                        cancellationToken: ct);
                    break;
            }
        }

        static Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken ct) // хендлинг ошибок
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}