using calledudeBot.Bots;
using calledudeBot.Chat.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot
{
    public static class calledudeBot
    {
        private static Hooky _hooky;

        private static void Main()
        {
            Console.Title = "calledudeBot";
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                var isCtrlC = e.SpecialKey == ConsoleSpecialKey.ControlC;
                var isCtrlBreak = e.SpecialKey == ConsoleSpecialKey.ControlBreak;

                if (isCtrlC || isCtrlBreak) e.Cancel = true;
            };
            CleanCmdFile();

            CredentialChecker.ProduceBots().Wait();
            var _bots = CredentialChecker.GetVerifiedBots(out var discordBot, out var twitchBot, out var osuBot);

            foreach (var bot in _bots)
                bot.Start();

            _hooky = new Hooky(ref twitchBot);
            new Thread(_hooky.Start).Start();
        }

        private static void CleanCmdFile()
        {
            if (!File.Exists(CommandUtils.CmdFile))
            {
                File.Create(CommandUtils.CmdFile).Close();
                return; //In this case, file is empty (newly created) -> no need for cleaning -> return
            }

            //Cleaning up
            List<string> cleanList = File.ReadAllLines(CommandUtils.CmdFile)
                                         .Where(p => !string.IsNullOrWhiteSpace(p))
                                         .Select(p => p.Trim())
                                         .ToList();

            File.WriteAllLines(CommandUtils.CmdFile, cleanList);
        }
    }
}
