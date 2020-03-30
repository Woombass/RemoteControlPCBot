using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace RemoteControlPCBot
{
    internal class BotCommandClass
    {
        public string Command { get; set; }
        public string Example { get; set; }
        public int CountArgs { get; set; }
        public Action<BotCommandModel,MessageEventArgs> Excecute { get; set; }
        public Action<BotCommandModel,MessageEventArgs> Error { get; set; }

        public static BotCommandModel ParseCommand (string text)
        {
            if (text.StartsWith("/"))
            {
                var splits = text.Split(' ');
                string name = splits?.FirstOrDefault();
                var args = splits.Skip(1).Take(splits.Count()).ToArray();

                return new BotCommandModel { Command = name, Args = args };
            }
            else
            {
                return null;
            }
        }
    }
}
