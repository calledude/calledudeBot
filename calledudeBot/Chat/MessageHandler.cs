using calledudeBot.Bots;
using calledudeBot.Chat.Info;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public class MessageHandler<T> where T : Message
    {
        private readonly Bot<T> _bot;
        private readonly CommandHandler<T> _commandHandler;

        public MessageHandler(Bot<T> bot)
        {
            if (Bot.TestRun) return;
            _commandHandler = new CommandHandler<T>(bot);
            _bot = bot;
        }

        private async Task Respond(T message) => await _bot.SendMessageAsync(message);

        public async Task<bool> DetermineResponse(T message)
        {
            var cmd = message.Content.Split(' ')[0];
            if (_commandHandler.IsPrefixed(cmd))
            {
                _bot.TryLog($"Handling message: {message.Content} from {message.Sender.Name} in {message.Channel}");
                var param = new CommandParameter(message.Content, message);
                await Respond(_commandHandler.GetResponse(param));
                return true;
            }
            return false;
        }
    }
}