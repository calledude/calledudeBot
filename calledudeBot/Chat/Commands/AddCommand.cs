using calledudeBot.Chat.Info;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Chat.Commands
{
    internal sealed class AddCommand : SpecialCommand<CommandParameter>
    {
        public AddCommand()
        {
            Name = "!addcmd";
            Description = "Adds a command to the command list";
            RequiresMod = true;
        }

        private static string createCommand(CommandParameter param)
        {
            try
            {
                Command cmd1 = CommandUtils.GetExistingCommand(param.PrefixedWords) ?? CommandUtils.GetExistingCommand(param.Words[0]);
                Command cmd2 = new Command(param);

                if (cmd1 is Command && cmd1.Name == cmd2.Name)
                {
                    return editCmd(cmd1, cmd2);
                }
                else if (cmd1 is Command && cmd1.Name != cmd2.Name)
                {
                    return "One or more of the alternate commands already exists.";
                }
                else
                {
                    //at this point we've tried everything, it doesn't exist, let's add it.
                    CommandUtils.Commands.Add(cmd2);
                    return $"Added command '{cmd2.Name}'";
                }
            }
            catch (ArgumentException e)
            {
                return e.Message;
            }
        }

        protected override Task<string> specialFunc(CommandParameter param)
        {
            string response;
            if (param.PrefixedWords.Count >= 1 || param.Words.Count >= 1) //has user entered a command to enter? i.e. !addcmd !test someAnswer
            {
                response = createCommand(param);
            }
            else
            {
                response = "You ok there bud? Try again.";
            }
            return Task.FromResult(response);
        }

        private static string editCmd(Command c, Command f)
        {
            string response;
            if (c is SpecialCommand || c is SpecialCommand<CommandParameter>)
                return "You can't change a special command.";

            int changes = 0;
            response = $"Command '{f.Name}' already exists.";

            if (f.Response != c.Response)
            {
                c.Response = f.Response;
                response = $"Changed response of '{c.Name}'.";
                changes++;
            }
            if (f.Description != c.Description)
            {
                c.Description = f.Description;
                response = $"Changed description of '{c.Name}'.";
                changes++;
            }
            if (f.AlternateName.Count != c.AlternateName.Count)
            {
                if (f.AlternateName.Count == 0)
                {
                    c.AlternateName = f.AlternateName;
                    response = $"Removed all alternate commands for {c.Name}";
                }
                else
                {
                    c.AlternateName.AddRange(f.AlternateName);
                    c.AlternateName = c.AlternateName.Distinct().ToList();
                    response = $"Changed alternate command names for {c.Name}. It now has {c.AlternateName.Count} alternates.";
                }
                changes++;
            }
            //Remove the new (wrongly) added new command from commandfile
            //and save the potentially new version.
            CommandUtils.RemoveCommand(f);
            return changes > 1 ? $"Done. Several changes made to command '{f.Name}'." : response;
        }
    }
}
