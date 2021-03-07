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

        public ResponseMessage(string trigger, string response, bool enabled, bool giveGot)
        {
            Trigger = trigger;
            Response = response;
            Enabled = enabled;
            GiveGot = GiveGot;
        }

        public int Id { get; set; }

        public string Trigger { get; set; }

        public string Response { get; set; }

        public bool Enabled { get; set; } = true;

        public bool GiveGot { get; set; } = true;
    }
}
