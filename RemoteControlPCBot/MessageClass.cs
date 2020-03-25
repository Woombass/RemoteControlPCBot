using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteControlPCBot
{


    /// <summary>
    /// Общий класс ответа сервера.
    /// </summary>
    public class MessageClass
    {
        /// <summary>
        /// Состояние.
        /// </summary>
        public bool ok { get; set; }
        /// <summary>
        /// Массив результатов.
        /// </summary>
        public Result[] result { get; set; }
    }

    public class Result
    {
        public long update_id { get; set; }
        public Message message { get; set; }
    }

    public class Message
    {
        public long message_id { get; set; }
        public From from { get; set; }
        public Chat chat { get; set; }
        public int date { get; set; }
        public string text { get; set; }
    }

    public class From
    {
        public long id { get; set; }
        public bool is_bot { get; set; }
        public string first_name { get; set; }
        public string username { get; set; }
        public string language_code { get; set; }
    }

    public class Chat
    {
        public long id { get; set; }
        public string first_name { get; set; }
        public string username { get; set; }
        public string type { get; set; }
    }
}
