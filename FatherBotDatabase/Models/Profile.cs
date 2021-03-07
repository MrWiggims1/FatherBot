using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FatherBotDatabase.Models
{
    public class Profile
    {
        public int Id { get; set; }

        public ulong DiscordId { get; set; }

        public int Gots { get; set; }
        public bool IsIgnored { get; set; }
        public int MessagesSent { get; set; }

        public double GotRatio => (MessagesSent > 0) ? Gots / MessagesSent : 0;
    }
}
