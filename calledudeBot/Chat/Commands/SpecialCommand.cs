using calledudeBot.Chat.Info;

namespace calledudeBot.Chat.Commands
{
    public abstract class SpecialCommand<T> : Command where T : CommandParameter
    {
        public abstract string Handle(T param);
    }

    public abstract class SpecialCommand : Command
    {
        public abstract string Handle();
    }
}
