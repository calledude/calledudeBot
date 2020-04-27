using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Chat.Info
{
    public abstract class CommandParameter
    {
        public List<string> PrefixedWords { get; }
        public IEnumerable<string> EnclosedWords { get; }
        public IEnumerable<string> Words { get; }

        protected CommandParameter(IEnumerable<string> param)
        {
            PrefixedWords = param
                .TakeWhile(x => x[0] == '!')
                .ToList();

            EnclosedWords = param
                .SkipWhile(x => !x.StartsWith("<"));

            Words = param
                .SkipWhile(x => x[0] == '!')
                .TakeWhile(x => !x.StartsWith("<"));
        }

        public abstract Task<bool> SenderIsMod();
    }

    public class CommandParameter<T> : CommandParameter, IRequest<T> where T : Message<T>
    {
        public T Message { get; }

        public CommandParameter(IEnumerable<string> param, T message) : base(param)
        {
            Message = message;
        }

        public override async Task<bool> SenderIsMod()
        {
            if (Message == null)
                return false;

            return await Message.Sender.IsModerator();
        }
    }
}