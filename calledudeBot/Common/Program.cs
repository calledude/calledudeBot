using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using calledudeBot.Bots;
using System.Threading.Tasks;
using System.Reflection;

namespace calledudeBot
{
    public class calledudeBot
    {
        public static string cmdFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\cmds.txt";
        public static OsuBot osuBot;
        public static DiscordBot discordBot;
        public static TwitchBot twitchBot;
        private static Hooky hooky;

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

            CredentialChecker c = new CredentialChecker();

            discordBot = c.VerifyBot(TestSubject.Discord) as DiscordBot;
            twitchBot = c.VerifyBot(TestSubject.Twitch) as TwitchBot;
            osuBot = c.VerifyBot(TestSubject.Osu) as OsuBot;
            Console.Clear();

            hooky = new Hooky(twitchBot);

            new Thread(async () => await discordBot.Start()).Start();
            new Thread(async () => await osuBot.Start()).Start();
            new Thread(async () => await twitchBot.Start()).Start();
            new Thread(hooky.Start).Start();
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
    }
}