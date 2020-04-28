using calledudeBot.Bots;
using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using calledudeBot.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public sealed class DiscordMessageHandler : MessageHandler<DiscordMessage>
    {
        public DiscordMessageHandler(ILogger<DiscordMessageHandler> logger, DiscordBot bot, MessageDispatcher dispatcher)
            : base(logger, bot, dispatcher)
        {
        }
    }

    public sealed class TwitchMessageHandler : MessageHandler<IrcMessage>
    {
        public TwitchMessageHandler(ILogger<TwitchMessageHandler> logger, TwitchBot bot, MessageDispatcher dispatcher)
            : base(logger, bot, dispatcher)
        {
        }
    }

    public abstract class MessageHandler<T> : INotificationHandler<T> where T : Message<T>
    {
        private readonly ILogger _logger;
        private readonly Bot<T> _bot;
        private readonly MessageDispatcher _dispatcher;

        protected MessageHandler(ILogger logger, Bot<T> bot, MessageDispatcher dispatcher)
        {
            _logger = logger;
            _bot = bot;
            _dispatcher = dispatcher;
        }

        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling message: {0} from {1} in {2}", notification.Content, notification.Sender.Name, notification.Channel);
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