using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace calledudeBot.Chat
{
    public class Command
    {
        private List<Command> commands = CommandHandler.commands;
        public delegate void Action<T1, T2>(T1 obj, out T2 obj2);
        private Action<Message> specialFunc;
        private string response;
        private string cmdFile = calledudeBot.cmdFile;
        private string arg;

        public string Name { get; }
        public string Response
        {
            get
            {
                if(IsSpecial) specialFunc.Invoke(Message);
                return response;
            }
            set { response = value; }
        }
        public string Description { get; set; }
        public bool IsSpecial { get; }
        public bool UserAllowed { get; set; }
        public string Arguments
        {
            set { arg = value; }
        }
        public string[] AlternateName { get; set; }
        public CommandHandler HandlerInstance { get; set; }
        public Message Message { get; set; }

        public Command(string cmd, string cmdToAdd, bool isSpecial = false, bool writeToFile = false)
        {
            if (cmd.Contains('<'))
            {
                int descriptionIndex = cmd.IndexOf('<');
                Description = cmd.Substring(descriptionIndex).Trim('<', '>');
                cmd = cmd.Remove(descriptionIndex);
            }
            if (!isSpecial)
            {
                int responseIndex = writeToFile ? cmd.IndexOf(cmd.Split(' ')[2]) : cmd.IndexOf(cmd.Split(' ')[1]);
                response = cmd.Substring(responseIndex).Trim();
            }
            else
            {
                IsSpecial = true;
                if (cmdToAdd == "!addcmd") specialFunc = addCmd;
                else if (cmdToAdd == "!delcmd") specialFunc = delCmd;
                else if (cmdToAdd == "!help") specialFunc = helpCmd;
                else if (cmdToAdd == "!np") specialFunc = playingCmd;
                else if (cmdToAdd == "!uptime") specialFunc = uptime;
            }
            UserAllowed = true;
            Name = cmdToAdd;
            
            if (writeToFile)
            {
                string line = Name + " " + Response + " <" + Description + ">";
                File.AppendAllText(cmdFile, line + Environment.NewLine);
            }

        }

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

        private void playingCmd(Message message)
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

            if (nowPlaying == null) response = "No song is playing right now.";
            else response = $"Song playing right now: {nowPlaying}";
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
                response = createCommand(cmd, cmdToAdd, true);
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

        private string createCommand(string cmd, string cmdToAdd, bool writeToFile)
        {
            string response = $"Added command '{cmdToAdd}'";
            Command f = new Command(cmd, cmdToAdd, false, writeToFile);
            foreach (Command c in commands)
            {
                if (c.Name == cmdToAdd)
                {
                    response = $"Command '{cmdToAdd}' already exists.";
                    if (f.Description != c.Description || f.Response != c.Response)
                    {
                        response = $"Changed command '{cmdToAdd}' successfully.";

                        if (string.IsNullOrEmpty(f.Description) && !string.IsNullOrEmpty(c.Description))
                        {
                            f.Description = c.Description;
                            response = $"Changed command {cmdToAdd} successfully but kept the description since you didn't provide any.";
                        }
                        commands.Add(f);
                        removeCommand(c); //No duplicates pls
                    }
                    return response; //Critical to return here since we otherwise would add it to the command list.
                }
            }
            commands.Add(f);
            return response;
        }

        private void removeCommand(Command cmd)
        {
            commands.Remove(cmd);
            List<string> cmds = new List<string>();

            foreach (Command c in commands)
            {
                if (c.IsSpecial) continue;
                string description = string.IsNullOrEmpty(c.Description) ? null : "<" + c.Description + ">";
                string line = c.Name + " " + c.Response + " " + description;
                cmds.Add(line);
            }
            File.WriteAllLines(cmdFile, cmds);

        }

        private void uptime(Message message)
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
