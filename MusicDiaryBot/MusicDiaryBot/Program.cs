using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MusicDiaryBot
{
    internal class Program
    {
        static TelegramBotClient botClient = new TelegramBotClient("8444705053:AAHtL9xr7Fu3P-XvOx-lzx4REHh8g0sOBxM");

        static Dictionary<long, UserState> userStates = new Dictionary<long, UserState>(); // словарь с состояниями всех активных пользователей
        
        static LibraryService libraryService = new LibraryService(); // обьект для работы с библиотекой
        static LastFmService lastFmService = new LastFmService(); // обьект для работы с lastfm

        static async Task Main(string[] args)
        {
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message } // разрешаем только сообщения
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
            if (update.Message == null) return; // проверка на null в сообщении

            long chatId = update.Message.Chat.Id; // айди для чата с пользователем


            if (update.Message.Text == null)
            {
                await HandleNonTextMessage(client, update.Message, chatId, ct);
                return;
            }
            
            string text = update.Message.Text;

            Console.WriteLine($"Получено сообщение от {chatId}: {text}");

            if (!userStates.ContainsKey(chatId)) // создание состояния для пользователя в словаре
                userStates[chatId] = new UserState();

            UserState state = userStates[chatId]; // текущее состояние

            try
            {
                if (state.State != DialogState.None) // если в процессе добавления трека
                {
                    await HandleAddDialog(client, chatId, text, state, ct);
                    return;
                }

                switch (text)
                {
                    case "/start":
                        await client.SendMessage(chatId,
                            "--- Добро пожаловать в Музыкальный дневник! ---\n\n" +
                            "Ты можешь добавить свои любимые треки в библиотеку, чтобы получать рекомендации\n\n" +
                            "Добавить треки можно через /add или просто пришли мне mp3 файл!" +
                            "/help чтобы увидеть список команд",
                            cancellationToken: ct);
                        break;

                    case "/help":
                        await client.SendMessage(chatId,
                            "--- Список команд: ---\n\n" +
                            "/add — добавить трек в библиотеку\n" +
                            "/list — посмотреть свою библиотеку\n" +
                            "/recommend — получить рекомендации\n" +
                            "/help — список команд",
                            cancellationToken: ct);
                        break;

                    case "/add":
                        state.State = DialogState.AwaitingArtistname; // перевод в режим диалога 
                        await client.SendMessage(chatId,
                            "Введи имя исполнителя:",
                            cancellationToken: ct);
                        break;

                    case "/list":
                        var tracks = libraryService.GetTracks(chatId);

                        if (tracks.Count == 0)
                        {
                            await client.SendMessage(chatId,
                                "Библиотека музыки еще пуста :p\n"
                                + "Добавь треки через /add",
                                cancellationToken: ct);
                            break;
                        }
                        string list = "--- Библиотека музыки ---\n\n";
                        foreach (var track in tracks)
                            list += track.ToStringForWrite() + '\n';
                        await client.SendMessage(chatId, list, cancellationToken: ct);
                        break;

                    case "/recommend":
                        var allTracks = libraryService.GetTracks(chatId);

                        if (allTracks.Count == 0)
                        {
                            await client.SendMessage(chatId, "Библиотека пуста :p\nДобавь треки через /add", cancellationToken: ct);
                            break;
                        }

                        Random r = new Random();
                        var randomTrack = allTracks[r.Next(allTracks.Count)];


                        var similarArtists = await lastFmService.GetSimilarArtistsAsync(randomTrack.Artistname);

                        if (similarArtists.Count == 0)
                        {
                            await client.SendMessage(chatId, $"К сожалению, найти похожих на {randomTrack.Artistname} исполнителей не удолось...", cancellationToken: ct);
                            break;
                        }
                        string recommendations = $"Тебе может понравиться, если слушаешь {randomTrack.Artistname}:\n\n";
                        for (int i = 0; i < similarArtists.Count; i++)
                            recommendations += $"{i + 1}. {similarArtists[i]}\n";

                        await client.SendMessage(chatId, recommendations, cancellationToken: ct);
                        break;

                        

                    default:
                        await client.SendMessage(chatId,
                            "Нет такой команды:(\n\n" +
                            "Напиши /help чтобы увидеть список доступных команд.",
                            cancellationToken: ct);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());

                await client.SendMessage(chatId,
                    "Произошла ошибка. Пожалуйста, попробуй ещё раз",
                    cancellationToken: ct);

                if (userStates.ContainsKey(chatId))
                    userStates[chatId].State = DialogState.None;
            }
        }

        static async Task HandleNonTextMessage(ITelegramBotClient client, Message message, long chatId, CancellationToken ct)
        {
            if (message.Type == MessageType.Audio)
            {
                Console.WriteLine("Прислан аудиофайл!!");

                string? artist = message.Audio?.Performer;
                string? title = message.Audio?.Title;

                if (title != null && artist != null)
                {
                    libraryService.AddTrack(chatId, new TrackEntry(artist, title));

                    await client.SendMessage(chatId,
                            $"✅ Трек добавлен!\n\n" +
                            $"{artist}\n" +
                            $"{title}",
                            cancellationToken: ct);
                }
                else
                {
                    await client.SendMessage(chatId,
                        "Не удалось прочитать теги трека.. Ты можешь добавить его вручную командой /add",
                        cancellationToken: ct);
                }
            }
            else
            {
                Console.WriteLine("Не текстовое соо");
                await client.SendMessage(chatId,
                        "Я могу распознать только текст и аудиофайлы :(\nПосмотреть команды - /help",
                        cancellationToken: ct);
            }
        }
        static async Task HandleAddDialog(ITelegramBotClient client, long chatId, string text, UserState state, CancellationToken ct)
        {
            switch (state.State)
            {
                case DialogState.AwaitingArtistname:
                    state.Artistname = text;
                    state.State = DialogState.AwaitingSongtitle;
                    await client.SendMessage(chatId,
                        "Введи название трека:",
                        cancellationToken: ct);
                    break;

                case DialogState.AwaitingSongtitle:
                    state.Songtitle = text;
                    state.State = DialogState.None;

                    libraryService.AddTrack(chatId, new TrackEntry(state.Artistname, state.Songtitle));

                    await client.SendMessage(chatId,
                        $"✅ Трек добавлен!\n\n" +
                        $"{state.Artistname}\n" +
                        $"{state.Songtitle}",
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