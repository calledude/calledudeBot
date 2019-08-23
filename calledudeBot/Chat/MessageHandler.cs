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

        protected async Task Respond(Message message)
            => await _bot.SendMessageAsync(message);

        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            var contentSplit = notification.Content.Split(' ');
            if (CommandUtils.IsCommand(contentSplit[0]))
            {
                _bot.Log($"Handling command: {notification.Content} from {notification.Sender.Name} in {notification.Channel}");
                var param = new CommandParameter(contentSplit, notification);

                await Respond(await _dispatcher.SendRequest(param));
            }
        }
    }
}