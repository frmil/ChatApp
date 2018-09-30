using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp.Models
{
    public class ChatMessage
    {
        private bool _myMessage;
        public ChatMessage(string message, bool myMessage = true)
        {
            Message = message;
            _myMessage = myMessage;
            if (_myMessage)
            {
                Background = "White";
                Margin = "0,0,50,0";
            } else
            {
                Background = "#FF4D9FF1";
                Margin = "50,0,0,0";
            }

        }

        public string Message { get; set; }
        public string Background { get; set; }
        public string Margin { get; set; }
    }
}
