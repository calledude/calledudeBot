using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using System.Collections.Generic;
using System.Linq;

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

        protected override string specialFunc(CommandParameter param)
        {
            string response = "You ok there bud? Try again.";
            var allowed = param.SenderIsMod;
            var cmdToHelp = param.PrefixedWords.FirstOrDefault() ?? param.Words.FirstOrDefault();
            if (cmdToHelp == null)
            {
                var availableCommands = CommandUtils.Commands
                                        .Where(x => !x.RequiresMod || allowed)
                                        .Select(x => x.Name);

                var commands = string.Join(" » ", availableCommands);

                response = $"These are the commands you can use: {commands}";
            }
            else if (CommandUtils.GetExistingCommand(cmdToHelp) is Command c) //"!help <command>"
            {
                if (c.RequiresMod && !allowed) return response;

                string cmds = c.Name;
                if (c.AlternateName.Count != 0)
                {
                    var alts = string.Join("/", c.AlternateName);
                    cmds += $"/{alts}";
                }
                string responseDescription = string.IsNullOrEmpty(c.Description)
                    ? "has no description."
                    : $"has description '{c.Description}'";
                response = $"Command '{cmds}' {responseDescription}";
            }

            return response;
        }
    }
}
