using calledudeBot.Chat.Info;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot.Chat.Commands
{
    public static class CommandUtils
    {
        internal static readonly string cmdFile = calledudeBot.cmdFile;
        internal static List<Command> Commands = new List<Command>();

        //Returns the Command object or null depending on if it exists or not.
        internal static Command GetExistingCommand(string cmd)
        {
            cmd = cmd.ToLower().AddPrefix();
            return Commands.Find(x => x.Name == cmd)
                ?? Commands.Find(x => x.AlternateName?.Any(a => a == cmd) ?? false);
        }

        internal static Command GetExistingCommand(IEnumerable<string> prefixedWords)
        {
            foreach (var word in prefixedWords)
            {
                if (GetExistingCommand(word) is Command c)
                    return c;
            }
            return null;
        }

        internal static void AppendCmdToFile(Command cmd)
        {
            if (cmd is SpecialCommand || cmd is SpecialCommand<CommandParameter>) return;

            string alternates = cmd.AlternateName.Count > 0 ? string.Join(" ", cmd.AlternateName) : null;
            string description = string.IsNullOrEmpty(cmd.Description) ? null : $"<{cmd.Description}>";
            string line = $"{cmd.Name} {cmd.Response}";

            if (alternates != null) line = $"{cmd.Name} {alternates} {cmd.Response}";
            if (description != null) line += " " + description;
            line = line.Trim();
            File.AppendAllText(cmdFile, line + Environment.NewLine);
        }

        internal static string RemoveCommand(Command cmd, string altName = null)
        {
            if (cmd is SpecialCommand)
                return "You can't change a special command.";

            string response;

            if (altName != cmd.Name)
            {
                cmd.AlternateName.Remove(altName);
                response = $"Deleted alternative command '{altName}'";
            }
            else
            {
                Commands.Remove(cmd);
                response = $"Deleted command '{altName}'";
            }

            File.Create(cmdFile).Close();
            foreach (Command c in Commands)
            {
                AppendCmdToFile(c);
            }
            return response;
        }

        internal static string AddPrefix(this string str)
        {
            return str[0] == '!' ? str : ("!" + str);
        }
    }
}
