using System;
using System.Collections.Generic;
using System.Linq;
using calledudeBot.Chat.Info;

namespace calledudeBot.Chat.Commands
{
    public class Command
    {
        public string Response { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool RequiresMod { get; protected set; }
        public List<string> AlternateName { get; set; }

        public Command(CommandParameter cmdParam)
        {
            Name = cmdParam.PrefixedWords.FirstOrDefault();
            if(Name == null)
            {
                Name = cmdParam.Words[0].AddPrefix();
                cmdParam.Words.RemoveAt(0);
            }

            AlternateName = cmdParam.PrefixedWords.Skip(1).ToList();
            Description = string.Join(" ", cmdParam.EnclosedWords).Trim('<', '>');
            Response = string.Join(" ", cmdParam.Words);

            if(cmdParam.PrefixedWords.Any(hasSpecialChars))
                throw new ArgumentException("Special characters in commands are not allowed.");

            CommandUtils.Commands.Add(this);
        }

        protected Command()
        {
            CommandUtils.Commands.Add(this);
        }

        private static bool hasSpecialChars(string str)
        {
            str = str[0] == '!' ? str.Substring(1) : str;
            return str.Any(c => !char.IsLetterOrDigit(c));
        }
    }
}
