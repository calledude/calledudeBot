using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot.Chat.Commands
{
    public static class CommandUtils
    {
        internal const char PREFIX = '!';
        internal static List<Command> Commands { get; set; } = new List<Command>();
        internal static string CmdFile { get; } = "commands.json";

        public static bool IsCommand(string message)
            => message[0] == PREFIX && message.Length > 1;

        //Returns the Command object or null depending on if it exists or not.
        internal static Command GetExistingCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return null;

            cmd = cmd.ToLower().AddPrefix();
            return Commands.Find(x => x.Name.Equals(cmd))
                ?? Commands.Find(x => x.AlternateName?.Any(a => a.Equals(cmd)) ?? false);
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

        internal static void SaveCommandsToFile()
        {
            var filteredCommands = Commands
                .Where(x => x.GetType() == typeof(Command));

            var commands =
                JsonConvert.SerializeObject(
                    filteredCommands,
                    Formatting.Indented,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    });
            File.WriteAllText(CmdFile, commands);
        }

        internal static string AddPrefix(this string str)
            => str[0] == PREFIX ? str : (PREFIX + str);
    }
}
