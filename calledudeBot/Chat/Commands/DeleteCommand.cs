using calledudeBot.Chat.Info;

namespace calledudeBot.Chat.Commands
{
    internal class DeleteCommand : SpecialCommand<CommandParameter>
    {
        public DeleteCommand()
        {
            Name = "!delcmd";
            Description = "Deletes a command from the command list";
            RequiresMod = true;
        }

        internal static string delCmd(CommandParameter param)
        {
            string response = "You ok there bud? Try again.";

            var cmdToDel = param.PrefixedWords[0];
            if (CommandUtils.GetExistingCommand(cmdToDel) is Command c)
            {
                response = CommandUtils.RemoveCommand(c, cmdToDel);
            }
            return response;
        }

        protected override string specialFunc(CommandParameter param)
        {
            string response = "You ok there bud? Try again.";

            var cmdToDel = param.PrefixedWords[0];
            if (CommandUtils.GetExistingCommand(cmdToDel) is Command c)
            {
                response = CommandUtils.RemoveCommand(c, cmdToDel);
            }
            return response;
        }
    }
}
