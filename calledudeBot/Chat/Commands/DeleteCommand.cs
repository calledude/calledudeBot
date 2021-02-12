using calledudeBot.Chat.Info;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Chat.Commands
{
    internal sealed class DeleteCommand : SpecialCommand<CommandParameter>
    {
        public DeleteCommand()
        {
            Name = "!delcmd";
            Description = "Deletes a command from the command list";
            RequiresMod = true;
        }

        protected override Task<string> HandleCommand(CommandParameter param)
        {
            var response = "You ok there bud? Try again.";

            var cmdToDel = param.PrefixedWords.FirstOrDefault()
                ?? param.Words.FirstOrDefault()?.AddPrefix();

            if (CommandUtils.GetExistingCommand(cmdToDel) is Command c)
            {
                response = RemoveCommand(c, cmdToDel);
            }

            return Task.FromResult(response);
        }

        private string RemoveCommand(Command cmd, string altName = null)
        {
            if (cmd is SpecialCommand)
                return "You can't remove a special command.";

            string response;

            if (altName != cmd.Name && altName != null)
            {
                cmd.AlternateName.Remove(altName);
                response = $"Deleted alternative command '{altName}'";
            }
            else
            {
                CommandUtils.Commands.Remove(cmd.Name);

                if (cmd.AlternateName != null)
                {
                    foreach (var alt in cmd.AlternateName)
                    {
                        CommandUtils.Commands.Remove(alt);
                    }
                }
                response = $"Deleted command '{altName}'";
            }

            CommandUtils.SaveCommandsToFile();

            return response;
        }
    }
}
