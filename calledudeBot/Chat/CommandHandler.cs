using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot.Chat
{
    public class CommandHandler
    {
        internal static List<Command> commands = new List<Command>();
        private string cmdFile = calledudeBot.cmdFile;
        private MessageHandler messageHandler;
        private static bool initialized;
        private static object m = new object();

        public CommandHandler(MessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
            lock (m)
            {
                if (!initialized) init();
            }
        }

        private void init()
        {
            initialized = true;
            var cmdArr = File.ReadAllLines(cmdFile);
            foreach (string line in cmdArr)
            {
                Command c = new Command(line);
                commands.Add(c);
            }
            commands.AddRange(new List<Command>
            {
                new Command("!addcmd <Adds a command to the command list>", true) { RequiresMod = true },
                new Command("!delcmd <Deletes a command from the command list>", true) { RequiresMod = true },
                new Command("!help <Lists all available commands>", true) { AlternateName = new List<string> { "!commands", "!cmds" } },
                new Command("!np <Shows which song is currently playing>", true) { AlternateName = new List<string> { "!song", "!playing" } },
                new Command("!uptime <Shows how long the stream has been live>", true)
            });
            Logger.log($"[CommandHandler]: Done. Loaded {commands.Count} commands.");
        }

        public bool isCommand(Message message)
        {
            return message.Content[0] == '!';
        }

        public Message getResponse(Message message)
        {
            string response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            var cmd = message.Content.Split(' ')[0].ToLower();

            foreach (Command c in commands)
            {
                if (cmd == c.Name || (c.AlternateName?.Any(x => cmd == x) ?? false))
                {
                    response = c.getResponse(message);
                    break; //Avoids exception if we were adding a command.
                }
            }
            message.Content = response;
            return message;
        }

    }
}
