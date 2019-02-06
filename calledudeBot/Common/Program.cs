﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using calledudeBot.Bots;
using System.Threading.Tasks;

namespace calledudeBot
{
    public static class calledudeBot
    {
        public static string cmdFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\cmds.txt";
        public static OsuBot osuBot;
        public static DiscordBot discordBot;
        public static TwitchBot twitchBot;
        private static Hooky hooky;
        private static List<Bot> bots;

        private static void Main()
        {
            Console.Title = "calledudeBot";
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                var isCtrlC = e.SpecialKey == ConsoleSpecialKey.ControlC;
                var isCtrlBreak = e.SpecialKey == ConsoleSpecialKey.ControlBreak;

                if (isCtrlC || isCtrlBreak) e.Cancel = true;
            };
            cleanCmdFile();

            CredentialChecker.ProduceBots();
            bots = CredentialChecker.GetVerifiedBots(out discordBot, out twitchBot, out osuBot);

            hooky = new Hooky(twitchBot);
            new Thread(hooky.Start).Start();

            Parallel.ForEach(bots, (bot) 
                => bot.Start());
        }

        private static void cleanCmdFile()
        {
            if (!File.Exists(cmdFile))
            {
                File.Create(cmdFile).Close();
                return; //In this case, file is empty (newly created) -> no need for cleaning -> return
            }

            //Cleaning up
            List<string> cleanList = File.ReadAllLines(cmdFile)
                                         .Where(p => !string.IsNullOrWhiteSpace(p))
                                         .Select(p => p.Trim())
                                         .ToList();

            File.WriteAllLines(cmdFile, cleanList);
        }
    }
}
