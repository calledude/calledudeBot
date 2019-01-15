using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using calledudeBot.Chat.Info;


namespace calledudeBot.Chat
{
    public partial class Command
    {
        internal static string helpCmd(CommandParameter param) //Implement different !help for discord?
        {
            string response = "You ok there bud? Try again.";
            var allowed = param.Message.Sender.isMod;
            var cmdToHelp = param.PrefixedWords.FirstOrDefault();
            if (cmdToHelp == null)
            {
                StringBuilder sb = new StringBuilder(commands.Count);

                foreach (Command c in commands)
                {
                    if (c.RequiresMod && !allowed) continue;
                    sb.Append(" " + c.Name + " »");
                }
                response = "These are the commands you can use:" + sb.ToString().Trim('»');
            }
            else if (getExistingCommand(cmdToHelp) is Command c) //"!help <command>"
            {
                if (c.RequiresMod && !allowed) return response;

                string cmds = c.Name;
                if (c.AlternateName.Count != 0)
                {
                    var alts = string.Join("/", c.AlternateName);
                    cmds += $"/{alts}";
                }
                string responseDescription = string.IsNullOrEmpty(c.Description)
                    ? "has no description."
                    : $"has description '{c.Description}'";
                response = $"Command '{cmds}' {responseDescription}";
            }

            return response;
        }

        internal static string playingCmd(CommandParameter param)
        {
            string nowPlaying = null;
            var procName = "osu!";
            var procList = Process.GetProcesses();

            foreach (var p in procList)
            {
                if (p.ProcessName == procName)
                {
                    nowPlaying = p.MainWindowTitle.Contains("-") ? p.MainWindowTitle.Substring(p.MainWindowTitle.IndexOf("-") + 1).Trim() : null;
                    break;
                }
            }
            return nowPlaying == null ? "No song is playing right now." : $"Song playing right now: {nowPlaying}";
        }

        internal static string addCmd(CommandParameter param)
        {
            if (param.PrefixedWords.Count >= 1 && param.Words.Count >= 1) //has user entered a command to enter? i.e. !addcmd !test someAnswer
            {
                return createCommand(param);
            }
            else
            {
                return "You ok there bud? Try again.";
            }
        }

        internal static string delCmd(CommandParameter param)
        {
            string response = "You ok there bud? Try again.";

            var cmdToDel = param.PrefixedWords.First();
            if (getExistingCommand(cmdToDel) is Command c)
            {
                response = removeCommand(c, cmdToDel);
            }
            return response;
        }

        //Returns the Command object or null depending on if it exists or not.
        public static Command getExistingCommand(string cmd)
        {
            cmd = cmd.ToLower();
            cmd = cmd[0] == '!' ? cmd : ("!" + cmd);
            return commands.FirstOrDefault(x => x.Name == cmd)
                ?? commands.FirstOrDefault(x => x.AlternateName.Any(a => a == cmd));

        }

        private static Command getExistingCommand(IEnumerable<string> prefixedWords)
        {
            foreach (var word in prefixedWords)
            {
                if (getExistingCommand(word) is Command c) return c;
            }
            return null;
        }

        internal static string createCommand(CommandParameter param)
        {
            try
            {
                Command cmd1 = getExistingCommand(param.PrefixedWords);
                Command cmd2 = new Command(param);

                if (cmd1 is Command && cmd1.Name == cmd2.Name)
                {
                    return editCmd(cmd1, cmd2);

                }
                else if (cmd1 is Command && cmd1.Name != cmd2.Name)
                {
                    return "One or more of the alternate commands already exists.";
                }
                else
                {
                    //at this point we've tried everything, it doesn't exist, let's add it.
                    commands.Add(cmd2);
                    return $"Added command '{cmd2.Name}'";
                }
            }
            catch (ArgumentException e)
            {
                return e.Message;
            }
        }

        internal static string editCmd(Command c, Command f)
        {
            string response;
            if (c.IsSpecial)
                return "You can't change a special command.";

            int changes = 0;
            response = $"Command '{f.Name}' already exists.";

            if (f.response != c.response)
            {
                c.response = f.response;
                response = $"Changed response of '{c.Name}'.";
                changes++;
            }
            if (f.Description != c.Description)
            {
                c.Description = f.Description;
                response = $"Changed description of '{c.Name}'.";
                changes++;
            }
            if (f.AlternateName.Count != c.AlternateName.Count)
            {
                if (f.AlternateName.Count == 0)
                {
                    c.AlternateName = f.AlternateName;
                    response = $"Removed all alternate commands for {c.Name}";
                }
                else
                {
                    c.AlternateName.AddRange(f.AlternateName);
                    c.AlternateName = c.AlternateName.Distinct().ToList();
                    response = $"Changed alternate command names for {c.Name}. It now has {c.AlternateName.Count} alternates.";
                }
                changes++;
            }
            //Remove the new (wrongly) added new command from commandfile
            //and save the potentially new version.
            removeCommand(f);
            return changes > 1 ? $"Done. Several changes made to command '{f.Name}'." : response;

        }

        private static string removeCommand(Command cmd, string altName = null)
        {
            if (cmd.IsSpecial)
                return "You can't change a special command.";

            string response;

            if (cmd.Name != altName)
            {
                cmd.AlternateName.Remove(altName);
                response = $"Deleted alternative command '{altName}'";
            }
            else
            {
                commands.Remove(cmd);
                response = $"Deleted command '{altName}'";
            }

            File.Create(cmdFile).Close();
            foreach (Command c in commands)
            {
                appendCmdToFile(c);
            }
            return response;

        }

        internal static void appendCmdToFile(Command cmd)
        {
            if (cmd.IsSpecial) return;
            string alternates = cmd.AlternateName.Any() ? string.Join(" ", cmd.AlternateName) : null;
            string description = string.IsNullOrEmpty(cmd.Description) ? null : $"<{cmd.Description}>";
            string line = $"{cmd.Name} {cmd.response}";
            if (alternates != null) line = $"{cmd.Name} {alternates} {cmd.response}";
            if (description != null) line += " " + description;
            line = line.Trim();
            File.AppendAllText(cmdFile, line + Environment.NewLine);
        }

        internal static string uptime(CommandParameter param)
        {
            DateTime d = calledudeBot.discordBot.wentLiveAt();
            TimeSpan t = DateTime.Now - d;
            if (default(DateTime) != d)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Stream uptime: ");
                if (t.Hours > 0) sb.Append($"{t.Hours}h ");
                if (t.Minutes > 0) sb.Append($"{t.Minutes}m ");
                if (t.Seconds > 0) sb.Append($"{t.Seconds}s");
                return sb.ToString();
            }
            return "Streamer isn't live.";

        }
    }
}
