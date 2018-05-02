using SpotifyAPI.Local;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;
using calledudeBot.Bots;

namespace calledudeBot.Common
{
    public class CommandHandler : Handler
    {
        private static List<Command> commands = new List<Command>();
        private TwitchBot twitch;
        private static List<string> mods = TwitchBot.mods;
        private string cmdFile = calledudeBot.cmdFile;
        private static bool initiated = false; //Let's make sure we don't read our commands several times, eh? :^)
        private bool allowed = false;
        private MessageHandler messageHandler;
        private static bool modCheckLock = true;
        private static Timer timer;
        private SpotifyLocalAPI spotify;

        public CommandHandler(MessageHandler messageHandler)
        {
            twitch = calledudeBot.twitchBot;
            this.messageHandler = messageHandler;
            if(!initiated) init();
        }

        private void modLockEvent(object sender, ElapsedEventArgs e)
        {
            modCheckLock = false;
            timer.Stop();
        }

        private void init()
        {
            initiated = true;
            var cmdArr = File.ReadAllLines(cmdFile);
            spotify = new SpotifyLocalAPI();
            spotify.Connect();
            foreach (string line in cmdArr)
            {
                createCommand(line, line.Split(' ')[0], false);
            }
            Command addCmd = new Command("addCmd <Adds a command to the command list>", "!addcmd", false, true);
            commands.Add(addCmd);
            Command help = new Command("helpCmd <Lists all available commands>", "!help", false, true);
            commands.Add(help);
            Command np = new Command("playingCmd <Shows which song is currently playing>", "!np", false, true);
            commands.Add(np);
            Command song = new Command("playingCmd <Shows which song is currently playing>", "!song", false, true);
            commands.Add(song);
            Command delCmd = new Command("delCmd <Deletes a command from the command list>", "!delcmd", false, true);
            commands.Add(delCmd);
            Command uptime = new Command("uptime <Shows how long the stream has been live>", "!uptime", false, true);
            commands.Add(uptime);

            timer = new Timer(30000);
            timer.Elapsed += modLockEvent;
            timer.Start();

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
            var user = message.Sender;
            allowed = isAllowed(user.ToLower());

            response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            foreach (Command c in commands)
            {
                if (cmd.ToLower().StartsWith(c.Name))
                {
                    c.Arguments = cmd;
                    c.handlerInstance = this;
                    if (c.IsSpecial && !c.UserAllowed)
                    {
                        response = "You're not allowed to use that command";
                        message.Content = allowed ? c.Response : response;
                        return message; //Critical to return here since we would otherwise let the user execute the command regardless if they're allowed or not.
                    }
                    response = c.Response;
                }
            }
            message.Content = response;
            return message;

        }

        private bool isAllowed(string user)
        {
            if (!modCheckLock)
            {
                twitch.updateMods();
                modCheckLock = true;
                timer.Start();
            }
            foreach(string m in mods)
            {
                if (m == user) return true;
            }
            return false;
        }

        private bool hasSpecialChars(string str)
        {
            str = str[0] == '!' ? str.Substring(1) : str;
            return str.Any(c => !Char.IsLetterOrDigit(c));
        }

        private void helpCmd(string cmd, out string response)
        {
            response = "";
            if (commands.Count == 0)
            {
                response = "There are no commands available at this time.";
                return;
            }
            //Implement different !help for discord?

            
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

                if (commands.Count > 0)
                {
                    foreach (Command c in commands)
                    {
                        if (!c.UserAllowed && !allowed) continue;
                        sb.Append(" " + c.Name + " »");
                    }
                    response = "These are the commands you can use:" + sb.ToString().Trim('»');
                }



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
                response = "Oi, mate, cut it off or I'll fucking shank you";
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
            Command f = new Command(cmd, cmdToAdd, writeToFile);
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

            for(int i = 0; i < commands.Count; i++)
            {
                if (commands[i].IsSpecial) continue;
                string description = string.IsNullOrEmpty(commands[i].Description) ? null : "<" + commands[i].Description + ">";
                string line = commands[i].Name + " " + commands[i].Response + " " + description;
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
                response = "Calledude has been live for: ";
                if (t.Hours > 0) response += $"{t.Hours}h";
                if (t.Minutes > 0) response += $"{t.Minutes}m";
                if (t.Seconds > 0) response += $"{t.Seconds}s";
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
                nowPlaying = spotify.GetStatus().Track.ArtistResource.Name + " - " + spotify.GetStatus().Track.TrackResource.Name;
            }
            return nowPlaying;
        }
    }
}
