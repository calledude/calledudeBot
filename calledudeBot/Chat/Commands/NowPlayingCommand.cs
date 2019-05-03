using System.Collections.Generic;
using System.Diagnostics;

namespace calledudeBot.Chat.Commands
{
    internal sealed class NowPlayingCommand : SpecialCommand
    {
        public NowPlayingCommand()
        {
            Name = "!np";
            AlternateName = new List<string> { "!song", "!playing" };
            Description = "Shows which song is currently playing";
            RequiresMod = false;
        }

        public override string Handle()
        {
            string nowPlaying = null;
            const string procName = "osu!";
            foreach (var p in Process.GetProcesses())
            {
                if (p.ProcessName.Equals(procName))
                {
                    nowPlaying = p.MainWindowTitle.Contains("-") ? p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1).Trim() : null;
                    break;
                }
            }
            return nowPlaying == null ? "No song is playing right now." : $"Song playing right now: {nowPlaying}";
        }
    }
}
