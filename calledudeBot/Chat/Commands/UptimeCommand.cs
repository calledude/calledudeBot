using calledudeBot.Chat.Info;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace calledudeBot.Chat.Commands
{
    internal class UptimeCommand : SpecialCommand
    {
        public UptimeCommand()
        {
            Name = "!uptime";
            Description = "Shows how long the stream has been live";
            AlternateName = new List<string> { "!live" };
            RequiresMod = false;
        }

        protected override string specialFunc()
        {
            DateTime d = calledudeBot.discordBot.WentLiveAt();
            TimeSpan t = DateTime.Now - d;
            if (default != d)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Stream uptime: ");
                if (t.Hours > 0) sb.Append(t.Hours).Append("h ");
                if (t.Minutes > 0) sb.Append(t.Minutes).Append("m ");
                if (t.Seconds > 0) sb.Append(t.Seconds).Append("s");
                return sb.ToString();
            }
            return "Streamer isn't live.";
        }
    }
}
