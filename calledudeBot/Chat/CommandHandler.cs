using SpotifyAPI.Local;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace calledudeBot.Chat
{
    public class CommandHandler : Handler
    {
        private static List<Command> commands = new List<Command>();
        
        private string cmdFile = calledudeBot.cmdFile;
        private static bool initiated = false; //Let's make sure we don't read our commands several times, eh? :^)
        private bool allowed = false;
        private MessageHandler messageHandler;
        private SpotifyLocalAPI spotify;

        public CommandHandler(MessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
            if(!initiated) init();
        }

        private void init()
        {
            initiated = true;
            spotify = new SpotifyLocalAPI();
            spotify.Connect();

            var cmdArr = File.ReadAllLines(cmdFile);
            foreach (string line in cmdArr)
            {
                createCommand(line, line.Split(' ')[0], false);
            }
            commands.AddRange(new List<Command>
            {
                new Command("<Adds a command to the command list>", "!addcmd", addCmd) { UserAllowed = false },
                new Command("<Deletes a command from the command list>", "!delcmd", delCmd) { UserAllowed = false },
                new Command("<Lists all available commands>", "!help", helpCmd) { AlternateName = new string[] { "!commands", "!cmds" } },
                new Command("<Shows which song is currently playing>", "!np", playingCmd){ AlternateName = new string[] { "!song", "!playing" } },
                new Command("<Shows how long the stream has been live>", "!uptime", uptime)
            });

            Console.WriteLine($"[CommandHandler]: Done. Loaded {commands.Count} commands.");
        }

        public CommandStatus determineCommand(Message message) // Is it a command?
        {
            if (message.Content.StartsWith("!"))
            {
                Message response = handleCommand(message);
                messageHandler.respond(response);
                return CommandStatus.Handled;
            }
            return CommandStatus.NotHandled;
        }

        private Message handleCommand(Message message)
        {
            string response;
            var cmd = message.Content;
            allowed = message.Sender.isMod;

            response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            foreach (Command c in commands)
            {
                if (cmd.ToLower().StartsWith(c.Name) || (c.AlternateName?.Any(x => cmd.ToLower().StartsWith(x)) ?? false))
                {
                    c.Arguments = cmd;
                    c.handlerInstance = this;
                    if (c.IsSpecial && !c.UserAllowed)
                    {
                        response = allowed ? c.Response : "You're not allowed to use that command";
                        break;
                    }
                    else
                    {
                        response = c.Response;
                        break;
                    }
                }
            }
            message.Content = response;
            return message;
        }

        private bool hasSpecialChars(string str)
        {
            str = str[0] == '!' ? str.Substring(1) : str;
            return str.Any(c => !Char.IsLetterOrDigit(c));
        }

        private void helpCmd(string cmd, out string response) //Implement different !help for discord?
        {
            response = "";
            if (commands.Count == 0)
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

        private void playingCmd(string cmd, out string response)
        {   
            var np = getNowPlaying();

            if (np == null) response = "No song is playing right now.";
            else response = $"Song playing right now: {np}";
        }

        private void addCmd(string cmd, out string response)
        {
            string cmdToAdd = cmd.Split(' ')[1].ToLower();
            if (hasSpecialChars(cmdToAdd))
            {
                response = "What are you trying to do? No mid-command special characters, please.";
                return;
            }
            if (cmd.Split(' ').Length > 2) //has user entered a command to enter? i.e. !addcmd !test someAnswer
            {
                cmdToAdd = cmdToAdd.StartsWith("!") ? cmdToAdd : ("!" + cmdToAdd);
                response = createCommand(cmd,cmdToAdd, true);
            }
            else
            {
                response = "You ok there bud? Try again.";
            }
        }

        private void delCmd(string cmd, out string response)
        {
            response = "You ok there bud? Try again.";
            cmd = cmd.ToLower();
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
            Command f = new Command(cmd, cmdToAdd, null, writeToFile);
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

            foreach(Command c in commands)
            {
                if (c.IsSpecial) continue;
                string description = string.IsNullOrEmpty(c.Description) ? null : "<" + c.Description + ">";
                string line = c.Name + " " + c.Response + " " + description;
                cmds.Add(line);
            }
            File.WriteAllLines(cmdFile, cmds);

        }

        private void uptime(string cmd, out string response)
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

        private string getNowPlaying()
        {
            var procName = "osu!";
            string nowPlaying = null;
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
                nowPlaying = spotify.GetStatus()?.Track.ArtistResource.Name + " - " + spotify.GetStatus().Track.TrackResource.Name;
            }
            return nowPlaying;
        }
    }
}
