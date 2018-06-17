﻿using SpotifyAPI.Local;
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
            spotify = new SpotifyLocalAPI();
            if(!spotify.Connect())
            {
                Console.WriteLine("[CommandHandler]: Couldn't connect to spotify.");
            }

            var cmdArr = File.ReadAllLines(cmdFile);
            foreach (string line in cmdArr)
            {
                Command c = new Command(line, line.Split(' ')[0]);
                commands.Add(c);
            }
            commands.AddRange(new List<Command>
            {
                new Command("<Adds a command to the command list>", "!addcmd", true) { UserAllowed = false },
                new Command("<Deletes a command from the command list>", "!delcmd", true) { UserAllowed = false },
                new Command("<Lists all available commands>", "!help", true) { AlternateName = new List<string> { "!commands", "!cmds" } },
                new Command("<Shows which song is currently playing>", "!np", true) { AlternateName = new List<string> { "!song", "!playing" } },
                new Command("<Shows how long the stream has been live>", "!uptime", true)
            });
            Console.WriteLine($"[CommandHandler]: Done. Loaded {commands.Count} commands.");
        }

        public CommandStatus determineCommand(Message message) // Is it a command?
        {
            if (!message.Content.StartsWith("!")) return CommandStatus.NotHandled;
            Message response = handleCommand(message);
            messageHandler.respond(response);
            return CommandStatus.Handled;
        }

        private Message handleCommand(Message message)
        {
            string response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            var cmd = message.Content;

            foreach (Command c in commands)
            {
                if (cmd.ToLower().StartsWith(c.Name) || (c.AlternateName?.Any(x => cmd.ToLower().StartsWith(x)) ?? false))
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
