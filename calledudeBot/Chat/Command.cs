using System;
using System.Collections.Generic;
using System.Linq;
using calledudeBot.Chat.Info;

namespace calledudeBot.Chat
{
    public partial class Command
    {
        private static List<Command> commands = CommandHandler.commands;
        private string response;
        private static string cmdFile = calledudeBot.cmdFile;
        public Func<CommandParameter, string> specialFunc;
        public string Name { get; }
        public string Description { get; set; }
        public bool IsSpecial { get; }
        public bool RequiresMod { get; } = false;
        public List<string> AlternateName { get; set; }

        public Command(CommandParameter cmdParam)
        {
            Name = cmdParam.PrefixedWords.First();
            AlternateName = cmdParam.PrefixedWords.Skip(1).ToList();
            Description = string.Join(" ", cmdParam.EnclosedWords).Trim('<', '>');
            response = string.Join(" ", cmdParam.Words);


            if(cmdParam.PrefixedWords.Any(x => hasSpecialChars(x)))
                throw new ArgumentException("Special characters in commands are not allowed.");

            //If the ctor is called with params that have a message associated with it,
            // we know it's from one of our bots, ergo, write to file.
            // This is because it otherwise would put hardcoded commands into the commandfile.
            if (cmdParam.Message != null)
                appendCmdToFile(this);
        }
        
        public Command(CommandParameter cmdParam, Func<CommandParameter, string> specialFunc, bool requiresMod = false) : this(cmdParam)
        {
            RequiresMod = requiresMod;
            this.specialFunc = specialFunc;
            IsSpecial = true;
        }
        public string getResponse(CommandParameter param)
        {
            return specialFunc?.Invoke(param) ?? response;
        }
        private static bool hasSpecialChars(string str)
        {
            str = str[0] == '!' ? str.Substring(1) : str;
            return str.Any(c => !char.IsLetterOrDigit(c));
        }

    }
}
