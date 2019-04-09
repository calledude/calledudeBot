using calledudeBot.Chat.Info;

namespace calledudeBot.Chat.Commands
{
    public abstract class SpecialCommand<T> : Command where T : CommandParameter
    {
        protected abstract string SpecialFunc(T param);
        public string GetResponse(T param) => SpecialFunc(param);
    }

    public abstract class SpecialCommand : Command
    {
        protected abstract string SpecialFunc();
        public string GetResponse() => SpecialFunc();
    }
}
