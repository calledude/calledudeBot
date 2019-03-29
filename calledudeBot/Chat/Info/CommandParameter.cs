using System.Collections.Generic;
using System.Linq;

namespace calledudeBot.Chat.Info
{
    public class CommandParameter
    {
        public List<string> PrefixedWords { get; } = new List<string>();
        public List<string> EnclosedWords { get; } = new List<string>();
        public List<string> Words { get; } = new List<string>();
        public Message Message { get; }
        public bool SenderIsMod => Message?.Sender.IsMod ?? false;

        public CommandParameter(IEnumerable<string> param, Message message)
        {
            PrefixedWords = param.Where(x => x[0] == '!').ToList();

            var paramStr = string.Join(" ", param);
            var encIdx = paramStr.LastIndexOf('<');
            var encEndIdx = paramStr.LastIndexOf('>') + 1;

            var encWords = encIdx > 1 && encEndIdx > 1 ? paramStr.Substring(encIdx, encEndIdx - encIdx) : null;
            EnclosedWords = encWords?.Split(' ')?.ToList() ?? EnclosedWords;

            Words = param.Except(EnclosedWords).Except(PrefixedWords).ToList();

            Message = message;

            PrefixedWords = PrefixedWords.Select(x => x = x[0] == '!' ? x : '!' + x).ToList();
        }

        //This ctor is used for offline initializations, e.g. CommandHandler at bootup.
        public CommandParameter(string param) : this(param.Split(' '), null)
        {
        }
    }
}