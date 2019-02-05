using calledudeBot.Chat.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using calledudeBot.Chat.Info;

namespace calledudeBot.Chat
{
    public class HelpCommand : SpecialCommand<CommandParameter>
    {
        public HelpCommand()
        {
            Name = "!help";
            AlternateName = new List<string> {"!commands", "!cmds"};
            Description = "Lists all available commands or helps you with a specific one.";
            RequiresMod = false;
        }

        protected override string specialFunc(CommandParameter param)
        {
            string response = "You ok there bud? Try again.";
            var allowed = param.Message.Sender.IsMod;
            var cmdToHelp = param.PrefixedWords.FirstOrDefault() ?? param.Words.FirstOrDefault();
            if (cmdToHelp == null)
            {
                StringBuilder sb = new StringBuilder(CommandUtils.Commands.Count);

                foreach (Command c in CommandUtils.Commands)
                {
                    if (c.RequiresMod && !allowed) continue;
                    sb.Append(" ").Append(c.Name).Append(" »");
                }
                response = "These are the commands you can use:" + sb.ToString().Trim('»');
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
