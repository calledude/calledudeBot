using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public sealed class HelpCommand : SpecialCommand<CommandParameter>
    {
        public HelpCommand()
        {
            Name = "!help";
            AlternateName = new List<string> { "!commands", "!cmds" };
            Description = "Lists all available commands or helps you with a specific one.";
            RequiresMod = false;
        }

        protected override async Task<string> HandleCommand(CommandParameter param)
        {
            const string errorResponse = "You ok there bud? Try again.";
            var cmdToHelp = param.PrefixedWords.FirstOrDefault() ?? param.Words.FirstOrDefault();

            if (cmdToHelp == null)
            {
                var availableCommands = CommandUtils.Commands
                                        .ToAsyncEnumerable()
                                        .WhereAwait(async x => !x.RequiresMod || await param.SenderIsMod())
                                        .Select(x => x.Name);

                var commands = string.Join(" » ", availableCommands.ToEnumerable());

                return $"These are the commands you can use: {commands}";
            }
            else if (CommandUtils.GetExistingCommand(cmdToHelp) is Command c) //"!help <command>"
            {
                if (c.RequiresMod && !await param.SenderIsMod())
                    return errorResponse;

                var cmds = c.Name;
                if (c.AlternateName?.Count > 0)
                {
                    var alts = string.Join("/", c.AlternateName);
                    cmds += $"/{alts}";
                }

                var responseDescription = string.IsNullOrEmpty(c.Description)
                    ? "has no description."
                    : $"has the description '{c.Description}'";

                return $"Command '{cmds}' {responseDescription}";
            }

            return errorResponse;
        }
    }
}
