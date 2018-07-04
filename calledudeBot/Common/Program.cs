using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using calledudeBot.Bots;

namespace calledudeBot
{
    public class calledudeBot
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

                if (isCtrlC) e.Cancel = true;
                else if (isCtrlBreak) e.Cancel = true;
            };
            Clean();

            CredentialChecker.ProduceBots();
            bots = CredentialChecker.GetVerifiedBots(out discordBot, out twitchBot, out osuBot);

            hooky = new Hooky(twitchBot);
            new Thread(hooky.Start).Start();

            foreach (Bot bot in bots)
            {
                new Thread(async () =>
                {
                    await bot.Start();
                    bot.StartServices();
                }).Start();
            }
        }



        private static void Clean()
        {
            if (!File.Exists(cmdFile))
            {
                File.Create(cmdFile).Close();
                return; //In this case, file is empty (newly created) -> no need for cleaning -> return
            }

            //Cleaning up
            var cleanUpArr = File.ReadAllLines(cmdFile).ToList();
            List<string> cleanList = cleanUpArr.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();

            File.WriteAllLines(cmdFile, cleanList);
        }


        private static void SetBotInstances(List<Bot> bots)
        {
            foreach(Bot bot in bots)
            {
                if (bot is DiscordBot d) discordBot = d;
                else if (bot is TwitchBot t) twitchBot = t;
                else if (bot is OsuBot o) osuBot = o;
            }
        }
    }
}
