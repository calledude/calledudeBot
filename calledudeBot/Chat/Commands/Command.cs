﻿using calledudeBot.Chat.Info;
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
        public List<string> AlternateName { get; set; } = new List<string>();

        public Command(CommandParameter cmdParam)
        {
            Name = cmdParam.PrefixedWords.FirstOrDefault();
            if (Name == null)
            {
                Name = cmdParam.Words[0].AddPrefix();
                cmdParam.Words.RemoveAt(0);
            }

            AlternateName = cmdParam.PrefixedWords.Skip(1).Distinct().ToList();
            Description = string.Join(" ", cmdParam.EnclosedWords).Trim('<', '>');
            Response = string.Join(" ", cmdParam.Words);

            if (cmdParam.PrefixedWords.Any(HasSpecialChars))
                throw new ArgumentException("Special characters in commands are not allowed.");
        }

        public Command()
        {
        }

        private static bool HasSpecialChars(string str)
        {
            str = str[0] == '!' ? str.Substring(1) : str;
            return str.Any(c => !char.IsLetterOrDigit(c));
        }
    }
}
