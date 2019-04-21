using MediatR;
using System.Collections.Generic;
using System.Linq;

namespace calledudeBot.Chat.Info
{
    public class CommandParameter : IRequest<Message>
    {
        public List<string> PrefixedWords { get; }
        public List<string> EnclosedWords { get; }
        public List<string> Words { get; }
        public Message Message { get; }
        public bool SenderIsMod => Message?.Sender.IsMod ?? false;

        public CommandParameter(IEnumerable<string> param, Message message)
        {
            PrefixedWords = param
                .TakeWhile(x => x[0] == '!')
                .ToList();

            EnclosedWords = param
                .SkipWhile(x => !x.StartsWith("<"))
                .ToList();

            Words = param
                .SkipWhile(x => x[0] == '!')
                .TakeWhile(x => !x.StartsWith("<"))
                .ToList();

            Message = message;
        }
    }
}