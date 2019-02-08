using calledudeBot.Bots;
using calledudeBot.Chat.Info;

namespace calledudeBot.Chat
{
    public class MessageHandler<T> where T : Message
    {
        private readonly Bot<T> bot;
        private readonly CommandHandler<T> commandHandler;

        public MessageHandler(Bot<T> bot)
        {
            if (Bot.testRun) return;
            commandHandler = new CommandHandler<T>(this);
            this.bot = bot;
        }

        private void respond(T message) => bot.SendMessage(message);

        public bool DetermineResponse(T message)
        {
            var msg = message.Content.Split(' ');
            var cmd = msg[0];
            if (commandHandler.IsPrefixed(cmd))
            {
                var param = new CommandParameter(msg, message);
                respond(commandHandler.GetResponse(param));
                return true;
            }
            return false;
        }
    }
}