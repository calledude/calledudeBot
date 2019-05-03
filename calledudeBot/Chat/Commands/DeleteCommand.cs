using calledudeBot.Chat.Info;
using System.Linq;

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

        public override string Handle(CommandParameter param)
        {
            string response = "You ok there bud? Try again.";

            var cmdToDel = param.PrefixedWords.FirstOrDefault() 
                ?? param.Words.FirstOrDefault()?.AddPrefix();

            if (CommandUtils.GetExistingCommand(cmdToDel) is Command c)
            {
                response = CommandUtils.RemoveCommand(c, cmdToDel);
            }
            return response;
        }
    }
}
