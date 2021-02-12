using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot.Chat.Commands
{
    public static class CommandUtils
    {
        internal const char PREFIX = '!';
        internal const string CMDFILE = "commands.json";

        internal static Dictionary<string, Command> Commands { get; set; } = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);

        public static bool IsCommand(string message)
            => message[0] == PREFIX && message.Length > 1;

        //Returns the Command object or null depending on if it exists or not.
        internal static Command GetExistingCommand(string cmd)
        {
            if (string.IsNullOrWhiteSpace(cmd))
                return null;

            if (!Commands.TryGetValue(cmd.AddPrefix(), out var command))
                return null;

            return command;
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

        public static void Add(this Dictionary<string, Command> commands, Command command)
        {
            commands.Add(command.Name, command);

            if (command.AlternateName == null)
                return;

            foreach (var alt in command.AlternateName)
            {
                commands.Add(alt, command);
            }
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

            File.WriteAllText(CMDFILE, commands);
        }

        internal static string AddPrefix(this string str)
            => str[0] == PREFIX ? str : (PREFIX + str);
    }
}
