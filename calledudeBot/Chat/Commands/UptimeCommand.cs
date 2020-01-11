using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace calledudeBot.Chat.Commands
{
    internal sealed class UptimeCommand : SpecialCommand
    {
        private readonly StreamMonitor _streamMonitor;

        public UptimeCommand(StreamMonitor streamMonitor)
        {
            Name = "!uptime";
            Description = "Shows how long the stream has been live";
            AlternateName = new List<string> { "!live" };
            RequiresMod = false;
            _streamMonitor = streamMonitor;
        }

        public override Task<string> Handle()
        {
            var wentLiveAt = WentLiveAt();
            if (wentLiveAt == default)
            {
                return Task.FromResult("Streamer isn't live.");
            }

            var timeSinceLive = DateTime.Now - wentLiveAt;
            var sb = new StringBuilder();

            sb.Append("Stream uptime: ");

            if (timeSinceLive.Hours > 0)
                sb.Append($"{timeSinceLive.Hours}h ");

            if (timeSinceLive.Minutes > 0)
                sb.Append($"{timeSinceLive.Minutes}m ");

            if (timeSinceLive.Seconds > 0)
                sb.Append($"{timeSinceLive.Seconds}s");

            return Task.FromResult(sb.ToString());
        }

        private DateTime WentLiveAt()
        {
            return _streamMonitor.IsStreaming
                ? _streamMonitor.StreamStarted
                : default;
        }
    }
}
