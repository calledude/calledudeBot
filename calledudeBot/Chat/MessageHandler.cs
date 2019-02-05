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

        public bool determineResponse(T message)
        {
            var msg = message.Content.Split(' ');
            var cmd = msg[0];
            if (commandHandler.isPrefixed(cmd))
            {
                var param = new CommandParameter(msg, message);
                respond(commandHandler.getResponse(param));
                return true;
            }
            return false;
        }
    }
}