using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using calledudeBot.Bots;

namespace calledudeBot.Common
{
    public class calledudeBot
    {
        public static string cmdFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\cmds.txt";
        public static string credFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\credentials";
        public static MessageHandler messageHandler;
        public static OsuBot osuBot;
        public static DiscordBot discordBot;
        public static TwitchBot twitchBot;
        private static Thread osuThread = new Thread(Osu);
        private static Thread discordThread = new Thread(Discord);
        private static Thread twitchThread = new Thread(Twitch);
        private static Thread hookyThread = new Thread(Hooky);
        private static string discordToken;
        private static string twitchAPItoken;
        private static string twitchIRCtoken;
        private static string osuIRCtoken;

        private static void Main(string[] args)
        {
            Clean();

            discordBot = new DiscordBot();
            osuBot = new OsuBot();
            twitchBot = new TwitchBot();

            getCredentials();

            discordThread.Start();
            osuThread.Start();
            twitchThread.Start();
            hookyThread.Start();
        }

        private static void getCredentials()
        {
            var credArr = File.ReadAllLines(credFile);
            List<string> credList = new List<string>(credArr);

            foreach(string s in credList)
            {
                if (s.StartsWith("DiscordToken")) discordToken = s.Split(' ')[1];
                else if (s.StartsWith("TwitchAPI")) twitchAPItoken = s.Split(' ')[1];
                else if (s.StartsWith("TwitchIRC")) twitchIRCtoken = s.Split(' ')[1];
                else if (s.StartsWith("osuIRC")) osuIRCtoken = s.Split(' ')[1];
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
            var cleanUpArr = File.ReadAllLines(cmdFile);
            var length = cleanUpArr.Length;

            List<string> cleanList = new List<string>();
            

            for (var i = 0; i < cleanUpArr.Length; i++)
            {
                var line = cleanUpArr[i];
                if (!string.IsNullOrWhiteSpace(line))
                {
                    cleanList.Add(line);
                }
            }

            File.WriteAllLines(cmdFile, cleanList);
        }

        private static async void Discord()
        {
            await discordBot.Start(discordToken, twitchAPItoken);
        }

        private static void Osu()
        {
            osuBot.Start(osuIRCtoken);
        }

        private static void Twitch()
        {
            twitchBot.Start(twitchIRCtoken);
        }

        private static void Hooky()
        {
            new Hooky(twitchBot).Start();
        }
    }
}