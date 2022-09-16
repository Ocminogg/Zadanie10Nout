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
using Telegram.Bot.Polling;


namespace WpfZadanie10
{
    class program
    {
        static ITelegramBotClient bot = new TelegramBotClient("5560152751:AAExhnTdlWOYWWBWoxRbDZrOubywSxMfnic");
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                var message = update.Message;
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Добро пожаловать на борт, добрый путник!");
                    return;
                }
                
            }
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            // Некоторые действия
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

    }
    

    class TelegramMessageClient
    {

        private MainWindow w;

        private TelegramBotClient bot;
        public ObservableCollection<MessageLog> BotMessageLog { get; set; }
        public MainWindow MainWindow { get; }

        public void MessageListener(object sender, ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
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
            this.BotMessageLog = new ObservableCollection<MessageLog>();
            this.w = W;
            this.bot = new TelegramBotClient(PathToken);

            var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions            
            {
                AllowedUpdates = { }, // receive all update types
            };
            //bot = new TelegramBotClient(PathToken);

            bot.StartReceiving(
            updateHandler: program.HandleUpdateAsync,
            pollingErrorHandler: program.HandleErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: cts.Token
            );

            

            
            //bot.OnMessage += MessageListener;
            
        }



        public void SendMessage(string Text, string Id)
        {
            long id = Convert.ToInt64(Id);
            bot.SendTextMessageAsync(id, Text);
        }



    }
}
