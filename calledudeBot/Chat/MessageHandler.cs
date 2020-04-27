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

    public abstract class MessageHandler<T> : INotificationHandler<T> where T : Message<T>
    {
        private readonly Bot<T> _bot;
        private readonly MessageDispatcher _dispatcher;

        protected MessageHandler(Bot<T> bot, MessageDispatcher dispatcher)
        {
            _bot = bot;
            _dispatcher = dispatcher;
        }

        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            Logger.Log($"Handling message: {notification.Content} from {notification.Sender.Name} in {notification.Channel}", this);
            var contentSplit = notification.Content.Split();
            if (CommandUtils.IsCommand(contentSplit[0]))
            {
                var param = new CommandParameter<T>(contentSplit, notification);
                var response = await _dispatcher.SendRequest(param);
                await _bot.SendMessageAsync(response);
            }
        }
    }
}