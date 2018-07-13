using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot.Chat
{
    public partial class Command
    {
        private List<Command> commands = CommandHandler.commands;
        private string response;
        private string cmdFile = calledudeBot.cmdFile;

        public string Name { get; }
        public string Description { get; set; }
        public bool IsSpecial { get; }
        public bool UserAllowed { get; set; }
        public List<string> AlternateName { get; set; }

        public Command(string cmd, string cmdToAdd, bool isSpecial = false, bool writeToFile = false)
        {
            if (cmd.Contains('<'))
            {
                int descriptionIndex = cmd.IndexOf('<');
                Description = cmd.Substring(descriptionIndex).Trim('<', '>');
                cmd = cmd.Remove(descriptionIndex);
            }
            if (!isSpecial) //We only set a response for non-special commands since special command responses are dynamic.
            {
                response = string.Join(" ", writeToFile ? cmd.Split(' ').Skip(2)
                                                        : cmd.Split(' ').Skip(1));
                response = response.Trim(); //Because fuck whitespaces amirite? :^)
            }

            IsSpecial = isSpecial;
            UserAllowed = true;
            Name = cmdToAdd;
            
            if (writeToFile)
            {
                string line = Name + " " + response + " <" + Description + ">";
                File.AppendAllText(cmdFile, line + Environment.NewLine);
            }
        }

        public string getResponse(Message message)
        {
            if (IsSpecial)
            {
                if (Name == "!addcmd") addCmd(message);
                else if (Name == "!delcmd") delCmd(message);
                else if (Name == "!help") helpCmd(message);
                else if (Name == "!np") playingCmd();
                else if (Name == "!uptime") uptime();
            }
            return response;
        }


    }
}
