using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public class TwitchCommandHandler : CommandHandler<IrcMessage>
    {
        public TwitchCommandHandler(ILogger<TwitchCommandHandler> logger) : base(logger) { }
    }

    public class DiscordCommandHandler : CommandHandler<DiscordMessage>
    {
        public DiscordCommandHandler(ILogger<DiscordCommandHandler> logger) : base(logger) { }
    }

    public class CommandHandler<T> : IRequestHandler<CommandParameter<T>, T> where T : Message<T>
    {
        private readonly ILogger _logger;

        public CommandHandler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<T> Handle(CommandParameter<T> request, CancellationToken cancellationToken)
        {
            string response;
            var cmd = CommandUtils.GetExistingCommand(request.PrefixedWords[0]);

            if (cmd == null)
            {
                response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            }
            else if (cmd.RequiresMod && !await request.SenderIsMod())
            {
                response = "You're not allowed to use that command";
            }
            else //Get the appropriate response depending on command-type
            {
                _logger.LogInformation("Executing command: {0}", cmd.Name);
                switch (cmd)
                {
                    case SpecialCommand<CommandParameter> sp:
                        response = await sp.Handle(request);
                        break;

                    case SpecialCommand s:
                        response = await s.Handle();
                        break;

                    default:
                        response = cmd.Response;
                        break;
                }
            }

            return request.Message.CloneWithMessage(response);
        }
    }
}
