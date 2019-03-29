using calledudeBot.Bots;
using System;
using System.Collections.Generic;
using System.Text;

namespace calledudeBot.Chat.Commands
{
    internal sealed class UptimeCommand : SpecialCommand
    {
        private readonly DiscordBot _discordbot;

        public UptimeCommand(DiscordBot discordbot)
        {
            Name = "!uptime";
            Description = "Shows how long the stream has been live";
            AlternateName = new List<string> { "!live" };
            RequiresMod = false;
            _discordbot = discordbot;
        }

        protected override string specialFunc()
        {
            DateTime d = _discordbot.WentLiveAt();
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
