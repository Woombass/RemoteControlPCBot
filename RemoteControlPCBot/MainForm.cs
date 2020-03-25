using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace RemoteControlPCBot
{
    

    public partial class MainForm : Form
    {
        private List<BotCommandClass> commands = new List<BotCommandClass>();
        TelegramBotClient botClient;
        private const long AdminId = 510746823;
        private const string Token = "1102558395:AAELDQQJbmF4bHFDo4dUPa2sdZoqZg2CLyk";
        private const string LogPath = "log";
        private bool flag = false;
        public MainForm()
        {
            InitializeComponent();
            Init();
            //this.WindowState = FormWindowState.Minimized;
            //MainForm_Deactivate(this,null);
        }


        ReplyKeyboardMarkup keyboard;

        private async void Init()
        {
            commands.Add
                (
                    new BotCommandClass 
                    { 
                        Command = "/start",
                        CountArgs = 0,
                        Example = "/start", 
                        Excecute = async (model,update) =>
                        {

                            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                            "Создатель бота CrandleXoder. Список доступных команд:\n"
                            + string.Join("\n", commands.Select(s => s.Example)));
                            KeyboardButton key1 = new KeyboardButton("/getScreen");
                            KeyboardButton key2 = new KeyboardButton("/reboot");
                            KeyboardButton key3 = new KeyboardButton("/turnOffBot");
                            KeyboardButton[] keyRow1 = { key1, key2 };
                            KeyboardButton[] keyRow = { key3 };
                            KeyboardButton[][] buttons = { keyRow, keyRow1 };
                            KeyboardButton[] keys = {  new KeyboardButton("/getScreen") ,  new KeyboardButton("/help") ,  new KeyboardButton("/off")  };
                            keyboard = new ReplyKeyboardMarkup(buttons,oneTimeKeyboard: true);
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, replyMarkup: keyboard,text: "Выбор действия:" );
                        },
                        Error = async (model, update) =>
                        {
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                                "Комманда введена неверно! Пробуйте так /start");
                        }
                    }
                );
            commands.Add
                (
                    new BotCommandClass
                    {
                        Command = "/getScreen",
                        CountArgs = 0,
                        Example="/getScreen",
                        Excecute = async (model, update) =>
                        {
                            ScreenShot();                                                           
                            using (var fs = File.OpenRead("screenShot.png"))
                            {
                                var file = new InputOnlineFile(fs, "screenShot.png");
                                await botClient.SendDocumentAsync(update.Message.Chat.Id, file, caption: "Скриншот в виде файла!");
                            }
                            WriteLog($"\r\n Отправлен скриншот пользователю {update.Message.Chat.FirstName} Дата: {DateTime.Now}");
                            File.Delete("screenShot.png");
                        },
                        Error = async (model, update) =>
                        {
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id,"Комманда введена неверно! Пробуйте так /getScreen");
                        }
                    }
                );

            commands.Add
                (
                    new BotCommandClass
                    {
                        Command = "/off",
                        CountArgs = 1,
                        Example = "/off [hh:mm]",
                        Excecute = async (model, update) => 
                        {
                            var time = DateTime.Parse(model.Args.FirstOrDefault());
                            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0,0);
                            TimeSpan seconds = time - origin;
                            Process.Start("shutdown", $"/s /t {seconds.TotalSeconds}");
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Компьютер будет выключен в указанное время.");
                            WriteLog($"Выключение компьютера в {time}");
                        },
                        Error = async (model, update) =>
                        {
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Комманда введена неверно! Пробуйте так /off 12:30");
                        }
                    }
                );

            commands.Add
                (
                    new BotCommandClass
                    {
                        Command = "/reboot",
                        CountArgs = 0,
                        Example = "/reboot",
                        Excecute = async (model, update) =>
                        {
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Начата перезагрузка ПК.");
                            WriteLog($"Выполняется перезагрузка. Запрос от пользователя: {update.Message.Chat.FirstName}. Дата: {DateTime.Now}");
                            Process.Start("shutdown", "/r /t 0");
                        },
                        Error = async (model, update) =>
                        {
                            await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Комманда введена неверно! Пробуйте так /reboot");
                        }
                    }
                );

            commands.Add
                (
                new BotCommandClass
                {
                    Command = "/turnOffBot",
                    Example = "/turnOffBot",
                    CountArgs = 0,
                    Excecute = async (model, update) =>
                    {
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Завершение работы бота.");
                        WriteLog($"\r\nВыключение бота. Запрос от пользователя: {update.Message.Chat.FirstName} Дата: {DateTime.Now} ");
                        Application.Exit();
                    }
                }
                );




            WebProxy proxy = new WebProxy("46.171.2.211", 3128);
            botClient = new TelegramBotClient(Token,proxy);

        
            var res = botClient.GetMeAsync().Result;
            textBoxLog.Text += res.FirstName + " Бот запущен!" ;

            botClient.OnMessage += BotClient_OnMessage;
            await botClient.SendTextMessageAsync(AdminId, "Бот запущен на: " + Environment.UserName);
            botClient.StartReceiving();

        }

        private void WriteLog(string text)
        {
            textBoxLog.Invoke( new Action(() => { textBoxLog.Text += text; }) );

            File.AppendAllText(LogPath + DateTime.Today.ToShortDateString()+".txt",text);
        }

        private async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (flag != true)
            {
                if (e.Message.Chat.Id == AdminId)
                {
                    if (e.Message.Text != null)
                    {
                        WriteLog("\r\n" + DateTime.Now + " " + e.Message.Chat.FirstName + "( " + e.Message.Chat.Id + " ): " + e.Message.Text);
                        var model = BotCommandClass.ParseCommand(e.Message.Text);
                        foreach (var cmd in commands)
                        {
                            if (cmd.Command == e.Message.Text)
                            {

                                if (cmd.CountArgs == model.Args.Length)
                                {
                                    cmd.Excecute?.Invoke(model, e);
                                }
                                else
                                {
                                    cmd.Error?.Invoke(model, e);
                                }

                            }
                        }


                    }
                }
                else
                {
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Запросите право пользования у администратора!");

                }
            }

        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            if (this.WindowState  == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Лог.Visible = true;
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.ShowInTaskbar = true;
                this.Лог.Visible = false;
                this.WindowState = FormWindowState.Normal;
            }
        }


        private void ScreenShot()
        {
            Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);

            Graphics graphics = Graphics.FromImage(printscreen as Image);

            graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
            printscreen.Save("screenShot.png", System.Drawing.Imaging.ImageFormat.Png);
            printscreen.Dispose();


        }







        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            botClient.SendTextMessageAsync(AdminId, "Бот остановлен!");


        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            botClient.SendTextMessageAsync(AdminId, "Бот остановлен!");
        }
    }
}
