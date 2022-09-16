using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using WpfZadanie10;
using Telegram.Bot.Polling;





var botClient = new TelegramBotClient("5560152751:AAExhnTdlWOYWWBWoxRbDZrOubywSxMfnic");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();



async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{

    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;
    //if (update.Type == UpdateType.Message && update?.Message?.Text != null)
    //{
    //    await HandleMessage(botClient, update.Message, cancellationToken);
    //    return;
    //}
    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;
    await HandleMessage(botClient, update.Message, cancellationToken);
    Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");



}

async Task HandleMessage(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
{
    if (message.Text == "/start")
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Choose commands: /inline | /keyboard");
        return;
    }

    if (message.Text == "/keyboard")
    {
        ReplyKeyboardMarkup keyboard = new(new[]
        {
            new KeyboardButton[] {"Hello", "Салам"},
            new KeyboardButton[] {"Привет", "Прощай дворф" },
            new KeyboardButton[] { "Отправь фото", "We" }
        })
        {
            ResizeKeyboard = true
        };
        await botClient.SendTextMessageAsync(message.Chat.Id, "Choose:", replyMarkup: keyboard);
        return;
    }

    List<string> mes = new List<string>() { "Hello", "Hi", "Салам", "Привет", "Здравствуй", "Здравствуйте", "Прощай дворф" };
    foreach (var me in mes)
    {
        if (me == message.Text)
        {
            if (message.Text == "Салам")
            {
                // Echo received message text
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Ас-саляму алейкум\n",
                    cancellationToken: cancellationToken);
                break;
            }
            if (message.Text == "Прощай дворф")
            {
                // Echo received message text
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Прощай, хорошего денёчка)\n",
                    cancellationToken: cancellationToken);

                Message sentVideo = await botClient.SendVideoAsync(
                chatId: message.Chat.Id,
                video: new InputOnlineFile("https://github.com/Ocminogg/zadanie9.4/raw/master/Media/Video/%D0%9F%D1%80%D0%BE%D1%89%D0%B0%D0%B9%D0%94%D0%B2%D0%BE%D1%80%D1%84%5BGoGetVideo.net%5D.mp4"),
                supportsStreaming: true,
                cancellationToken: cancellationToken);
                break;
            }
            else
            {
                // Echo received message text
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Здравствуй\n",
                    cancellationToken: cancellationToken);
                break;
            }
        }
    }

    if (message.Text == "We")
    {
        Message messageMusic = await botClient.SendAudioAsync(
        chatId: message.Chat.Id,
        audio: "https://minty.club/artist/daft-punk/get-lucky-feat-pharrell-williams-and-nile-rodgers/daft-punk-get-lucky-feat-pharrell-williams-and-nile-rodgers.mp3",

        cancellationToken: cancellationToken);
    }


    if (message.Text == "Отправь фото")
    {
        Message messagePhoto = await botClient.SendPhotoAsync(
        chatId: message.Chat.Id,
        photo: "https://github.com/TelegramBots/book/raw/master/src/docs/photo-ara.jpg",
        caption: "<b>Ara bird</b>. <i>Source</i>: <a href=\"https://pixabay.com\">Pixabay</a>",
        parseMode: ParseMode.Html,
        cancellationToken: cancellationToken);
    }

    if (message.Type == MessageType.Document)
    {
        Console.WriteLine(message.Document.FileId);
        Console.WriteLine(message.Document.FileName);
        Console.WriteLine(message.Document.FileSize);

        DownLoad(message.Document.FileId, message.Document.FileName);
    }

    async void DownLoad(string fileId, string path)
    {
        var file = await botClient.GetFileAsync(fileId);
        FileStream fs = new FileStream("_" + path, FileMode.Create);
        await botClient.DownloadFileAsync(file.FilePath, fs);
        fs.Close();

        fs.Dispose();
    }

    //await botClient.SendTextMessageAsync(message.Chat.Id, $"You said:\n{message.Text}");
}
/////////////////////////////////////////////////////////

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}






namespace WpfZadanie10
{
    class TelegramMessageClient
    {

        private MainWindow w;

        private TelegramBotClient bot;
        public ObservableCollection<MessageLog> BotMessageLog { get; set; }
        public MainWindow MainWindow { get; }

        public void MessageListener(object sender, ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            Console.WriteLine("---");
            Debug.WriteLine("+++---");

            string text = $"{DateTime.Now.ToLongTimeString()}: {message.Chat.FirstName} {message.Chat.Id} {message.Text}";

            Debug.WriteLine($"{text} TypeMessage: {message.Type.ToString()}");

            if (message.Text == null) return;

            var messageText = message.Text;

            w.Dispatcher.Invoke(() =>
            {
                BotMessageLog.Add(
                new MessageLog(
                    DateTime.Now.ToLongTimeString(), messageText, message.Chat.FirstName, message.Chat.Id));
            });
        }

        public TelegramMessageClient(MainWindow W, string PathToken = "5560152751:AAExhnTdlWOYWWBWoxRbDZrOubywSxMfnic")
        {
            var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions();

            this.BotMessageLog = new ObservableCollection<MessageLog>();
            this.w = W;

            bot = new TelegramBotClient("5560152751:AAExhnTdlWOYWWBWoxRbDZrOubywSxMfnic");

            //bot.OnMessage += MessageListener;

            bot.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions, 
            cancellationToken: cts.Token
            );

        }



        public void SendMessage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            bot.SendTextMessageAsync(id, Text);
        }



    }
}
