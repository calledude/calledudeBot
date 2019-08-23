using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace calledudeBot.Chat.Info
{
    public class CommandParameter : IRequest<Message>
    {
        public List<string> PrefixedWords { get; }
        public IEnumerable<string> EnclosedWords { get; }
        public IEnumerable<string> Words { get; }
        public Message Message { get; }
        public bool SenderIsMod => Message?.Sender.IsMod ?? false;

        public CommandParameter(IEnumerable<string> param, Message message)
        {
            PrefixedWords = param
                .TakeWhile(x => x[0] == '!')
                .ToList();

            EnclosedWords = param
                .SkipWhile(x => !x.StartsWith("<"));

            Words = param
                .SkipWhile(x => x[0] == '!')
                .TakeWhile(x => !x.StartsWith("<"));

            Message = message;
        }
    }
}