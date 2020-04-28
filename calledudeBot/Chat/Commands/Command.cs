using calledudeBot.Chat.Info;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (cmdParam.PrefixedWords.Any(HasSpecialChars))
                throw new ArgumentException("Special characters in commands are not allowed.");

            Name = cmdParam.PrefixedWords.FirstOrDefault();

            var alts = cmdParam
                .PrefixedWords
                .Skip(1)
                .Distinct();

            AlternateName = alts.Any()
                ? alts.ToList()
                : AlternateName;

            Description = string.Join(" ", cmdParam.EnclosedWords)
                                .Trim('<', '>');

            Response = string.Join(" ", cmdParam.Words);
        }

        public Command()
        {
        }

        private static bool HasSpecialChars(string str)
        {
            str = str[0] == CommandUtils.PREFIX ? str.Substring(1) : str;
            return !str.All(char.IsLetterOrDigit);
        }
    }
}
