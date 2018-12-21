using System;
using System.Collections.Generic;
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
        public bool RequiresMod { get; set; } = false;
        public List<string> AlternateName { get; set; }

        public Command(string cmd, bool isSpecial = false, bool writeToFile = false)
        {
            var cmds = cmd.Split(' ').Where((x,i) => x[0] == '!' || i == 0).ToArray();
            var altIndex = cmds.Length > 1 ? cmd.IndexOf(cmds[1]) : -1;
            if (altIndex > cmd.IndexOf(cmds[0]))
            {
                AlternateName = cmds.Skip(1).ToList();
                cmd = cmd.Remove(altIndex);
            }

            if (cmd.Contains('<'))
            {
                int descriptionIndex = cmd.LastIndexOf('<');
                int descLen = cmd.LastIndexOf('>') - descriptionIndex;
                Description = cmd.Substring(descriptionIndex, descLen).Trim('<', '>');
                cmd = cmd.Remove(descriptionIndex);
            }

            if (!isSpecial) //We only set a response for non-special commands since special command responses are dynamic.
            {
                response = string.Join(" ", cmd.Split(' ').Skip(1));
                response = response.Trim(); //Because fuck whitespaces amirite? :^)
            }

            var cmdToAdd = cmds[0];
            Name = cmdToAdd[0] == '!' ? cmdToAdd : '!' + cmdToAdd;
            IsSpecial = isSpecial;

            if (hasSpecialChars(Name) || (AlternateName != null && AlternateName.Any(x => hasSpecialChars(x))))
                throw new ArgumentException("Special characters in command are not allowed");

            if (writeToFile)
                appendCmdToFile(this);
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


        private bool hasSpecialChars(string str)
        {
            str = str[0] == '!' ? str.Substring(1) : str;
            return str.Any(c => !char.IsLetterOrDigit(c));
        }

    }
}
