using calledudeBot.Chat.Info;

namespace calledudeBot.Chat.Commands
{
    public abstract class SpecialCommand<T> : Command where T : CommandParameter
    {
        public string Handle(T param)
        {
            //Remove whatever command they were executing from PrefixedWords e.g. !addcmd
            param.PrefixedWords.RemoveAt(0);
            return Handle(param);
        }

        protected abstract string HandleCommand(T param);
    }

    public abstract class SpecialCommand : Command
    {
        public abstract string Handle();
    }
}
