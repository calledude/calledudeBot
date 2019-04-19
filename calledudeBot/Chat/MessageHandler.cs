using calledudeBot.Bots;
using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using calledudeBot.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public sealed class DiscordMessageHandler : MessageHandler<DiscordMessage>
    {
        public DiscordMessageHandler(DiscordBot bot, MessageDispatcher dispatcher)
            : base(bot, dispatcher)
        {
        }
    }

    public sealed class TwitchMessageHandler : MessageHandler<IrcMessage>
    {
        public TwitchMessageHandler(TwitchBot bot, MessageDispatcher dispatcher) 
            : base(bot, dispatcher)
        {
        }
    }

    public abstract class MessageHandler<T> : INotificationHandler<T> where T : Message
    {
        private readonly Bot<T> _bot;
        private readonly MessageDispatcher _dispatcher;

        protected MessageHandler(Bot<T> bot, MessageDispatcher dispatcher)
        {
            _bot = bot;
            _dispatcher = dispatcher;
        }

        protected async Task Respond(T message)
            => await _bot.SendMessageAsync(message);

        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            var cmd = notification.Content.Split(' ')[0];
            if (CommandUtils.IsCommand(cmd))
            {
                _bot.Log($"Handling command: {notification.Content} from {notification.Sender.Name} in {notification.Channel}");
                var param = new CommandParameter(notification.Content, notification);

                await Respond((T)await _dispatcher.SendRequest(param));
            }
        }
    }
}