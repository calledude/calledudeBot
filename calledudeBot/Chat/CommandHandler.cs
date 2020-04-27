using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using calledudeBot.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public sealed class CommandHandler : IRequestHandler<CommandParameter, Message>
    {


        public async Task<Message> Handle(CommandParameter request, CancellationToken cancellationToken)
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
