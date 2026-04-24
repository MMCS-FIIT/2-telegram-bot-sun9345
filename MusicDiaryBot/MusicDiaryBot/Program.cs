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

            await client.SendMessage(chatId, $"Ты написал: {text}", cancellationToken: ct); // отправка сообщений
        }

        static Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken ct) // хендлинг ошибок
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}