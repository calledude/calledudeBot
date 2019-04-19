using calledudeBot.Chat.Commands;
using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace calledudeBot.Chat.Info
{
    public class CommandParameter : IRequest<Message>
    {
        public List<string> PrefixedWords { get; } = new List<string>();
        public List<string> EnclosedWords { get; } = new List<string>();
        public List<string> Words { get; } = new List<string>();
        public Message Message { get; }
        public bool SenderIsMod => Message?.Sender.IsMod ?? false;

        public CommandParameter(string param, Message message)
        {
            var paramSplit = param.Split(' ');
            PrefixedWords = paramSplit.Where(x => x[0] == '!').ToList();

            var encIdx = param.LastIndexOf('<');
            var encEndIdx = param.LastIndexOf('>') + 1;

            var encWords = encIdx > 1 && encEndIdx > 1
                            ? param.Substring(encIdx, encEndIdx - encIdx)
                            : null;

            EnclosedWords = encWords?
                            .Split(' ')?
                            .ToList() 
                            ?? EnclosedWords;

            Words = paramSplit
                    .Except(EnclosedWords)
                    .Except(PrefixedWords)
                    .ToList();

            Message = message;

            PrefixedWords = PrefixedWords
                            .Select(x => x.AddPrefix())
                            .ToList();
        }
    }
}