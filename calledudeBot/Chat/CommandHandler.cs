using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using calledudeBot.Chat.Info;

namespace calledudeBot.Chat
{
    public class CommandHandler
    {
        internal static List<Command> commands;
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
            commands = cmdArr.Select(x => new Command(new CommandParameter(x))).ToList();

            commands.AddRange(new List<Command>
            {
                new Command(new CommandParameter("!addcmd <Adds a command to the command list>"), Command.addCmd, true),
                new Command(new CommandParameter("!delcmd <Deletes a command from the command list>"), Command.delCmd, true),
                new Command(new CommandParameter("!help !commands !cmds <Lists all available commands>"), Command.helpCmd),
                new Command(new CommandParameter("!np !song !playing <Shows which song is currently playing>"), Command.playingCmd),
                new Command(new CommandParameter("!uptime !live <Shows how long the stream has been live>"), Command.uptime)
            });
            Logger.log($"[CommandHandler]: Done. Loaded {commands.Count} commands.");
        }

        public bool isPrefixed(string message)
        {
            return message[0] == '!';
        }

        public Message getResponse(CommandParameter param)
        {
            string response;
            if (Command.getExistingCommand(param.PrefixedWords.First()) is Command c) //Does the command exist?
            {
                if (c.RequiresMod && !param.Message.Sender.isMod)
                {
                    response = "You're not allowed to use that command";
                }
                else
                {
                    param.PrefixedWords.RemoveAt(0); //Remove whatever command they were executing from PrefixedWords e.g. !addcmd
                    response = c.getResponse(param);
                }
            }
            else
            {
                response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            }
            param.Message.Content = response;
            return param.Message;
        }

    }
}
