using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace calledudeBot.Chat
{
    public partial class Command
    {
        private bool hasSpecialChars(string str)
        {
            str = str[0] == '!' ? str.Substring(1) : str;
            return str.Any(c => !Char.IsLetterOrDigit(c));
        }

        private void helpCmd(Message message) //Implement different !help for discord?
        {
            var allowed = message.Sender.isMod;
            var cmd = message.Content;
            if (CommandHandler.commands.Count == 0)
            {
                response = "There are no commands available at this time.";
                return;
            }

            if (cmd.Split(' ').Length == 2) //"!help <command>"
            {
                cmd = cmd.Split(' ')[1].ToLower();
                cmd = cmd.StartsWith("!") ? cmd : ("!" + cmd);
                foreach (Command c in commands)
                {
                    if (c.Name == cmd)
                    {
                        if (!c.UserAllowed && !allowed) return;
                        string responseDescription = string.IsNullOrEmpty(c.Description) ? " has no description." : (" has description '" + c.Description + "'");
                        response = $"Command '{c.Name}' {responseDescription}";
                    }
                }
            }
            else if (cmd.Split(' ').Length == 1) //"!help" only
            {
                StringBuilder sb = new StringBuilder(commands.Count);

                foreach (Command c in commands)
                {
                    if (!c.UserAllowed && !allowed) continue;
                    sb.Append(" " + c.Name + " »");
                }
                response = "These are the commands you can use:" + sb.ToString().Trim('»');
            }
        }

        private void playingCmd()
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

            if (string.IsNullOrEmpty(nowPlaying))
            {
                nowPlaying = CommandHandler.spotify.GetStatus()?.Track.ArtistResource.Name + " - " + CommandHandler.spotify.GetStatus().Track.TrackResource.Name;
            }

            response = nowPlaying == null ? "No song is playing right now." : $"Song playing right now: {nowPlaying}";
        }

        private void addCmd(Message message)
        {
            if (!message.Sender.isMod)
            {
                response = "You're not allowed to use that command";
                return;
            }

            string cmdToAdd = message.Content.Split(' ')[1].ToLower();
            string cmd = message.Content;
            if (cmd.Split(' ').Length > 2 && !hasSpecialChars(cmdToAdd)) //has user entered a command to enter? i.e. !addcmd !test someAnswer
            {
                cmdToAdd = cmdToAdd.StartsWith("!") ? cmdToAdd : ("!" + cmdToAdd);
                response = createCommand(cmd, cmdToAdd);
            }
            else
            {
                response = "You ok there bud? Try again.";
            }
        }

        private void delCmd(Message message)
        {
            if (!message.Sender.isMod)
            {
                response = "You're not allowed to use that command";
                return;
            }
            response = "You ok there bud? Try again.";

            var cmd = message.Content.ToLower();
            string cmdToDel = cmd.Split(' ')[1];
            cmdToDel = cmdToDel.StartsWith("!") ? cmdToDel : ("!" + cmdToDel);

            foreach (Command c in commands)
            {
                if (c.Name == cmdToDel)
                {
                    removeCommand(c);
                    response = $"Deleted command '{cmdToDel}'";
                    break;
                }
            }
        }

        private string createCommand(string cmd, string cmdToAdd)
        {
            string response;
            Command f = new Command(cmd, cmdToAdd, false, true);

            foreach (Command c in commands)
            {
                if (c.Name != f.Name) continue;
                
                //Command already exists, lets change it.
                if (c.IsSpecial)
                {
                    response = "You can't change a special command.";
                    return response;
                }
                if (f.response != c.response && f.Description != c.Description)
                {
                    c.Description = f.Description ?? c.Description; //Keep description if new description is null
                    c.response = f.response;
                    response = $"Changed command '{c.Name}' successfully.";
                }
                else if (f.response != c.response)
                {
                    c.response = f.response;
                    response = $"Changed response of '{c.Name}' successfully";
                }
                else if (f.Description != c.Description)
                {
                    c.Description = f.Description;
                    response = $"Changed description of '{c.Name}' successfully.";
                }
                else
                {
                    response = $"Command '{c.Name}' already exists.";
                }
                removeCommand(f);
                return response; //We don't want to add the command back, now do we? :^)
            }
            commands.Add(f);
            return $"Added command '{f.Name}'";
        }

        private void removeCommand(Command cmd)
        {
            commands.Remove(cmd);
            List<string> cmds = new List<string>();

            foreach (Command c in commands)
            {
                if (c.IsSpecial) continue;
                string description = string.IsNullOrEmpty(c.Description) ? null : "<" + c.Description + ">";
                string line = c.Name + " " + c.response + " " + description;
                cmds.Add(line);
            }
            File.WriteAllLines(cmdFile, cmds);

        }

        private void uptime()
        {
            DateTime d = calledudeBot.discordBot.wentLiveAt();
            TimeSpan t = DateTime.Now - d;
            response = "Streamer isn't live.";
            if (default(DateTime) != d)
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("Stream uptime: ");
                if (t.Hours > 0) sb.Append($"{t.Hours}h ");
                if (t.Minutes > 0) sb.Append($"{t.Minutes}m ");
                if (t.Seconds > 0) sb.Append($"{t.Seconds}s");
                response = sb.ToString();
            }
        }
    }
}
