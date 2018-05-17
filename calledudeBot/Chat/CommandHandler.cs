using SpotifyAPI.Local;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot.Chat
{
    public class CommandHandler : Handler
    {
        internal static List<Command> commands = new List<Command>();
        internal static SpotifyLocalAPI spotify;
        private string cmdFile = calledudeBot.cmdFile;
        private bool allowed = false;
        private MessageHandler messageHandler;

        public CommandHandler(MessageHandler messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        public void init()
        {
            spotify = new SpotifyLocalAPI();
            if(!spotify.Connect())
            {
                Console.WriteLine("[CommandHandler]: Couldn't connect to spotify.");
            }

            var cmdArr = File.ReadAllLines(cmdFile);
            foreach (string line in cmdArr)
            {
                commands.Add(new Command(line, line.Split(' ')[0]));
            }
            commands.AddRange(new List<Command>
            {
                new Command("<Adds a command to the command list>", "!addcmd", true) { UserAllowed = false },
                new Command("<Deletes a command from the command list>", "!delcmd", true) { UserAllowed = false },
                new Command("<Lists all available commands>", "!help", true) { AlternateName = new string[] { "!commands", "!cmds" } },
                new Command("<Shows which song is currently playing>", "!np", true) { AlternateName = new string[] { "!song", "!playing" } },
                new Command("<Shows how long the stream has been live>", "!uptime", true)
            });
            Console.WriteLine($"[CommandHandler]: Done. Loaded {commands.Count} commands.");
        }

        public CommandStatus determineCommand(Message message) // Is it a command?
        {
            if (message.Content.StartsWith("!"))
            {
                Message response = handleCommand(message);
                messageHandler.respond(response);
                return CommandStatus.Handled;
            }
            return CommandStatus.NotHandled;
        }

        private Message handleCommand(Message message)
        {
            string response;
            var cmd = message.Content;

            response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            foreach (Command c in commands)
            {
                if (cmd.ToLower().StartsWith(c.Name) || (c.AlternateName?.Any(x => cmd.ToLower().StartsWith(x)) ?? false))
                {
                    c.HandlerInstance = this;
                    c.Message = message;
                    response = c.Response;
                    break; //Avoids exception if we were adding a command.
                }
            }
            message.Content = response;
            return message;
        }


    }
}
