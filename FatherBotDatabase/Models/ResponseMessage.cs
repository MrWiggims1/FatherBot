using System;
using System.Collections.Generic;
using System.Text;

namespace FatherBot.Models
{
    public class ResponseMessage
    {
        public ResponseMessage()
        {

        }

        public ResponseMessage(string trigger, string response)
        {
            Trigger = trigger;
            Response = response;
        }

        public int Id { get; set; }

        public string Trigger { get; set; }

        public string Response { get; set; }

        public long Enabled { get; set; } = 1;

        public long GiveGot { get; set; } = 1;
    }
}
