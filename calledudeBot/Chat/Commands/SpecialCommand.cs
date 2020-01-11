using calledudeBot.Chat.Info;
using System.Threading.Tasks;

namespace calledudeBot.Chat.Commands
{
    public abstract class SpecialCommand<T> : Command where T : CommandParameter
    {
        public async Task<string> Handle(T param)
        {
            //Remove whatever command they were executing from PrefixedWords e.g. !addcmd
            param.PrefixedWords.RemoveAt(0);
            return await HandleCommand(param);
        }

        protected abstract Task<string> HandleCommand(T param);
    }

    public abstract class SpecialCommand : Command
    {
        public abstract Task<string> Handle();
    }
}
