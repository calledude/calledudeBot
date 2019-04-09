using calledudeBot.Bots;
using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using calledudeBot.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot.Chat
{
    public abstract class CommandHandler
    {
        protected readonly string CmdFile = CommandUtils.CmdFile;
        protected static bool Initialized;
        protected static readonly object Lock = new object();
    }

    public sealed class CommandHandler<T> : CommandHandler where T : Message
    {
        public CommandHandler(Bot<T> bot)
        {
            lock (Lock)
            {
                if (!Initialized) Initialize();
            }
            if (bot is DiscordBot discord)
            {
                CommandUtils.Commands.Add(new UptimeCommand(discord));
                Logger.Log($"[CommandHandler]: Done. Loaded {CommandUtils.Commands.Count} commands.");
            }
        }

        private void Initialize()
        {
            Initialized = true;
            var cmdArr = File.ReadAllLines(CmdFile);
            CommandUtils.Commands = cmdArr.Select(x => new Command(new CommandParameter(x))).ToList();
            CommandUtils.Commands.AddRange(new List<Command>
            {
                new AddCommand(),
                new DeleteCommand(),
                new HelpCommand(),
                new NowPlayingCommand()
            });
        }

        public bool IsPrefixed(string message) => message[0] == '!';

        public T GetResponse(CommandParameter param)
        {
            string response;
            var cmd = CommandUtils.GetExistingCommand(param.PrefixedWords[0]);

            if (cmd == null)
            {
                response = "Not sure what you were trying to do? That is not an available command. Try '!help' or '!help <command>'";
            }
            else if (cmd.RequiresMod && !param.SenderIsMod)
            {
                response = "You're not allowed to use that command";
            }
            else //Get the appropriate response depending on command-type
            {
                switch (cmd)
                {
                    case SpecialCommand<CommandParameter> sp:
                        param.PrefixedWords.RemoveAt(0); //Remove whatever command they were executing from PrefixedWords e.g. !addcmd
                        response = sp.GetResponse(param);
                        break;
                    case SpecialCommand s:
                        response = s.GetResponse();
                        break;
                    default:
                        response = cmd.Response;
                        break;
                }
            }
            param.Message.Content = response;
            return (T)param.Message;
        }
    }
}
