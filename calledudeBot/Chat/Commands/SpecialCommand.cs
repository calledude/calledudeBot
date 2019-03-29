using calledudeBot.Chat.Info;
using System.Threading.Tasks;

namespace calledudeBot.Chat.Commands
{
    public abstract class SpecialCommand<T> : Command where T : CommandParameter
    {
        protected abstract Task<string> specialFunc(T param);
        public virtual async Task<string> GetResponse(T param) => await specialFunc(param);
    }

    public abstract class SpecialCommand : Command
    {
        protected abstract string specialFunc();
        public virtual string GetResponse() => specialFunc();
    }
}
