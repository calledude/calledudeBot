using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using calledudeBot.Chat;

namespace calledudeBot.Chat.Info
{
    public class CommandParameter
    {
        private List<string> prefixedWords = new List<string>();

        public List<string> PrefixedWords
        {
            get
            {
                return prefixedWords = prefixedWords.Distinct().ToList();
            }
            private set
            {
                prefixedWords = value;
            }
        }
        public List<string> EnclosedWords { get; } = new List<string>();
        public List<string> Words { get; private set; } = new List<string>();
        public Message Message { get; }
        public bool SenderIsMod { get; }

        public CommandParameter(IEnumerable<string> param, Message message)
        {
            PrefixedWords = param.Where((x, i) => x[0] == '!').ToList();
            
            //If message is null, it was most likely called from an offline state, 
            // ergo, it doesn't have the !addcmd-part, and as such we won't add the second word 
            // as if it were intended as a shorthand (non-prefixed) commandname
            if(message != null && param.Skip(1).Any())
                PrefixedWords.Add(param.Skip(1).First());

            var paramStr = string.Join(" ", param);
            var encIdx = paramStr.LastIndexOf('<');
            var encEndIdx = paramStr.LastIndexOf('>') + 1;

            var encWords = encIdx > 1 && encEndIdx > 1 ? paramStr.Substring(encIdx, encEndIdx - encIdx) : null;
            EnclosedWords = encWords?.Split(' ')?.ToList() ?? EnclosedWords;

            Words = param.Except(EnclosedWords).Except(PrefixedWords).ToList();

            Message = message;

            SenderIsMod = message?.Sender.isMod ?? false;

            PrefixedWords = PrefixedWords.Select(x => x = x[0] == '!' ? x : '!' + x).ToList();

        }

        //This ctor is used for offline initializations, e.g. CommandHandler at bootup.
        public CommandParameter(string param) : this(param.Split(' '), null)
        {

        }
    }
}