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
        private void helpCmd(Message message) //Implement different !help for discord?
        {
            var allowed = message.Sender.isMod;
            var cmd = message.Content;
            if (CommandHandler.commands.Count == 0)
            {
                response = "There are no commands available at this time.";
                return;
            }
            response = "You ok there bud? Try again.";

            if (cmd.Split(' ').Length == 2) //"!help <command>"
            {
                cmd = cmd.Split(' ')[1].ToLower();
                cmd = cmd.StartsWith("!") ? cmd : ("!" + cmd);
                foreach (Command c in commands)
                {
                    if (c.Name == cmd || (c.AlternateName?.Any(x => cmd == x) ?? false))
                    {
                        if (c.AlternateName != null)
                        {
                            var alts = string.Join("/", c.AlternateName);
                            cmd = $"{c.Name}/{alts}";
                        }
                        if (c.RequiresMod && !allowed) return;
                        string responseDescription = string.IsNullOrEmpty(c.Description) 
                            ? "has no description." 
                            : $"has description '{c.Description}'";
                        response = $"Command '{cmd}' {responseDescription}";
                    }
                }
            }
            else if (cmd.Split(' ').Length == 1) //"!help" only
            {
                StringBuilder sb = new StringBuilder(commands.Count);

                foreach (Command c in commands)
                {
                    if (c.RequiresMod && !allowed) continue;
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
            response = nowPlaying == null ? "No song is playing right now." : $"Song playing right now: {nowPlaying}";
        }

        private void addCmd(Message message)
        {
            if (!message.Sender.isMod)
            {
                response = "You're not allowed to use that command";
                return;
            }
            
            var cmd = message.Content.Split(' ');
            if (cmd.Length > 2) //has user entered a command to enter? i.e. !addcmd !test someAnswer
            {
                response = createCommand(cmd);
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
                else if(c.AlternateName?.Any(x => cmdToDel == x) ?? false)
                {
                    removeCommand(c, true, cmdToDel);
                    response = $"Deleted alternative command '{cmdToDel}'";
                    break;
                }
            }
        }

        //The most inefficient piece of code ever seen by man
        private string createCommand(string[] cmd)
        {
            string cmdInfo = string.Join(" ", cmd.Skip(1)); //Skip '!addcmd' part
            Command f = new Command(cmdInfo, false, true);

            //Flatten all existing alternate names into a single collection
            var allAlternates = commands.Where(x => x.AlternateName != null).SelectMany(x => x.AlternateName); 

            //Checks if the alternative commands already exists
            if ((f.AlternateName?.Any(alt => allAlternates.Contains(alt)) ?? false) || allAlternates.Any(x => x == f.Name))
            {
                removeCommand(f);
                return "One or more of the alternate commands already exists.";
            }
            else if(commands.Select(x => x.Name).Any(x => x == f.Name))
            {
                return editCmd(f);
            }

            //at this point we've tried everything, it doesn't exist, let's add it.
            commands.Add(f);
            return $"Added command '{f.Name}'";
        }

        private string editCmd(Command f)
        {
            int changes = 0;
            foreach (Command c in commands)
            {
                if (c.Name != f.Name) continue;
                response = $"Command '{f.Name}' already exists.";
                
                //Found existing command, lets change it.
                if (c.IsSpecial)
                {
                    response = "You can't change a special command.";
                    break;
                }
                if (f.AlternateName != c.AlternateName)
                {
                    c.AlternateName = c.AlternateName ?? new List<string>();
                    f.AlternateName = f.AlternateName ?? new List<string>();

                    if (f.AlternateName.Count == 0)
                        c.AlternateName = f.AlternateName;
                    else
                    {
                        c.AlternateName.AddRange(f.AlternateName);
                        c.AlternateName = c.AlternateName.Distinct().ToList();
                    }
                    response = $"Changed alternate command names for {c.Name}. It now has {c.AlternateName.Count} alternates.";
                    changes++;
                }
                if (f.response != c.response)
                {
                    c.response = f.response;
                    response = $"Changed response of '{c.Name}' successfully.";
                    changes++;
                }
                else if (f.Description != c.Description)
                {
                    c.Description = f.Description;
                    response = $"Changed description of '{c.Name}' successfully.";
                    changes++;
                }
                else if (f.response != c.response && f.Description != c.Description)
                {
                    c.Description = f.Description ?? c.Description; //Keep description if new description is null
                    c.response = f.response;
                    response = $"Changed command '{c.Name}' successfully.";
                    changes++;
                }
            }
            removeCommand(f); //Remove the newly (wrongly) added new command
            return changes > 1 ? $"Done. Several changes made to command '{f.Name}'." : response; 

        }

        private void removeCommand(Command cmd, bool isAlternate = false, string altName = null)
        {
            if (isAlternate) cmd.AlternateName.Remove(altName);
            else commands.Remove(cmd);

            File.Create(cmdFile).Close();
            foreach (Command c in commands)
            {
                appendCmdToFile(c);
            }

        }

        private void appendCmdToFile(Command cmd)
        {
            if (cmd.IsSpecial) return;
            string alternates = cmd.AlternateName == null ? null : string.Join(" ", cmd.AlternateName);
            string description = string.IsNullOrEmpty(cmd.Description) ? null : $"<{cmd.Description}>";
            string line = $"{cmd.Name} {cmd.response}";
            if (description != null) line += " " + description;
            if (alternates != null) line += " " + alternates;
            line = line.Trim();
            File.AppendAllText(cmdFile, line + Environment.NewLine);
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
