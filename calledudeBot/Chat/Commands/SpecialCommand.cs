using calledudeBot.Chat.Info;

namespace calledudeBot.Chat.Commands
{
    public abstract class SpecialCommand<T> : Command where T : CommandParameter
    {
        protected abstract string Handle(T param);
        public string GetResponse(T param) => Handle(param);
    }

    public abstract class SpecialCommand : Command
    {
        protected abstract string Handle();
        public string GetResponse() => Handle();
    }
}
