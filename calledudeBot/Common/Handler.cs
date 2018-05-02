namespace calledudeBot.Common
{
    public class Handler
    {
        public CommandHandler commandHandler;
        public enum CommandStatus
        {
            Handled, NotHandled, NeedsAttention
        }

    }
}
