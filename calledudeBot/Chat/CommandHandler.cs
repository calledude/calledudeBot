using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using calledudeBot.Services;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public class TwitchCommandHandler : CommandHandler<IrcMessage>
    {
    }

    public class DiscordCommandHandler : CommandHandler<DiscordMessage>
    {
    }

    public class CommandHandler<T> : IRequestHandler<CommandParameter<T>, T> where T : Message<T>
    {
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
                Logger.Log($"Executing command: {cmd.Name}");
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
