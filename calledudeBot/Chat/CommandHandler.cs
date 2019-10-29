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
        private readonly string _cmdFile = CommandUtils.CmdFile;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Initialize()
        {
            CommandUtils.Commands = 
                JsonConvert.DeserializeObject<List<Command>>(File.ReadAllText(_cmdFile)) 
                ?? new List<Command>();
            CommandUtils.Commands.AddRange(_serviceProvider.GetServices<Command>());
            Logger.Log($"Done. Loaded {CommandUtils.Commands.Count} commands.", this);
        }

        public Task<Message> Handle(CommandParameter request, CancellationToken cancellationToken)
        {
            string response;
            var cmd = CommandUtils.GetExistingCommand(request.PrefixedWords[0]);

            if (cmd == null)
            {
                response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            }
            else if (cmd.RequiresMod && !request.SenderIsMod)
            {
                response = "You're not allowed to use that command";
            }
            else //Get the appropriate response depending on command-type
            {
                switch (cmd)
                {
                    case SpecialCommand<CommandParameter> sp:
                        //Remove whatever command they were executing from PrefixedWords e.g. !addcmd
                        request.PrefixedWords.RemoveAt(0);
                        response = sp.Handle(request);
                        break;
                    case SpecialCommand s:
                        response = s.Handle();
                        break;
                    default:
                        response = cmd.Response;
                        break;
                }
            }

            return Task.FromResult(request.Message.CloneWithMessage(response));
        }
    }
}
