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
        private static string credFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\credentials";
        public static OsuBot osuBot;
        public static DiscordBot discordBot;
        public static TwitchBot twitchBot;
        private static Hooky hooky;
        private static string discordToken, twitchAPItoken, twitchIRCtoken, osuIRCtoken, 
                                osuAPIToken, botNick, channelName, osuNick, announceChanID;

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

            getCredentials();
            Clean();

            discordBot = new DiscordBot(discordToken, twitchAPItoken, channelName, announceChanID);
            osuBot = new OsuBot(osuIRCtoken, osuNick);
            twitchBot = new TwitchBot(twitchIRCtoken, osuAPIToken, osuNick, botNick, channelName);
            hooky = new Hooky(twitchBot);

            new Thread(discordBot.Start).Start();
            new Thread(osuBot.Start).Start();
            new Thread(twitchBot.Start).Start();
            new Thread(hooky.Start).Start();
        }
        
        private static void openWebsite(int delay, string url) //Opens a website in a non-blocking manner after the specified delay
        {
            Task.Run(() =>
            {
                Thread.Sleep(delay);
                Process.Start(url);
                return Task.CompletedTask;
            });
        }
        
        private static bool getConfirmation()
        {
            ConsoleKey c = ConsoleKey.A;
            while (c != ConsoleKey.Y && c != ConsoleKey.N)
            {
                c = Console.ReadKey(true).Key;
            }
            Console.WriteLine();

            return c == ConsoleKey.Y;
        }

        private static void getBotNick(Dictionary<string, string> creds)
        {
            Console.Write("What will the username be of your bot be?: ");
            botNick = Console.ReadLine();
            creds.Add("BotNick", botNick);
        }

        private static void getChannelName(Dictionary<string, string> creds)
        {
            Console.Write("Will you be using your personal twitch account as the 'bot'? Y/N: ");
            if (getConfirmation()) channelName = "#" + botNick;
            else
            {
                Console.Write("Please enter your nickname on twitch: ");
                channelName = "#" + Console.ReadLine();
            }
            creds.Add("ChannelName", channelName);
        }

        private static void getOsuNick(Dictionary<string, string> creds)
        {
            Console.Write("What's your osu! username?: ");
            osuNick = Console.ReadLine();
            creds.Add("OsuNick", osuNick);
        }

        private static void getAnnounceChannelID(Dictionary<string, string> creds)
        {
            Console.Write("What channel on your discord server do you want the announcements to be made on? (Long number): ");
            openWebsite(4000, "https://support.discordapp.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID");

            announceChanID = Console.ReadLine();
            creds.Add("AnnounceChannelID", announceChanID);
        }

        private static void getDiscordToken(Dictionary<string, string> creds)
        {
            Console.WriteLine("First you need to create a bot/app on the discord developer website.");
            Console.Write("Discord token: ");
            openWebsite(3000, "https://discordapp.com/developers/applications/me");

            discordToken = Console.ReadLine();
            Console.Write("Have you figured out how to get the bot to join your own channel? Y/N: ");
            
            if (!getConfirmation())
            {
                Console.Write("Alright, enter the bots Client-ID here: ");
                openWebsite(0, $"https://discordapp.com/oauth2/authorize?client_id={Console.ReadLine()}&scope=bot");
            }
            creds.Add("DiscordToken", discordToken);
        }

        private static void getTwitchAPI(Dictionary<string, string> creds)
        {
            Console.Write("Twitch API token (Called 'Client ID' on twitch): ");
            openWebsite(2000, "https://dev.twitch.tv/dashboard/apps");
            twitchAPItoken = Console.ReadLine();
            creds.Add("TwitchAPI", twitchAPItoken);
        }

        private static void getTwitchIRC(Dictionary<string, string> creds)
        {
            Console.Write("Twitch IRC token: ");
            openWebsite(2000, "http://www.twitchapps.com/tmi/");
            twitchIRCtoken = Console.ReadLine();
            creds.Add("TwitchIRC", twitchIRCtoken);
        }

        private static void getosuIRC(Dictionary<string, string> creds)
        {
            Console.Write("osu! IRC password: ");
            openWebsite(2000, "https://osu.ppy.sh/p/irc");
            osuIRCtoken = Console.ReadLine();
            creds.Add("osuIRC", osuIRCtoken);
        }

        private static void getosuAPI(Dictionary<string, string> creds)
        {
            Console.Write("osu! API key: ");
            openWebsite(2000, "https://osu.ppy.sh/p/api");
            osuAPIToken = Console.ReadLine();
            creds.Add("osuAPI", osuAPIToken);
        }

        private static bool tryLoadCredentials(Dictionary<string, string> creds)
        {
            return creds.TryGetValue("BotNick", out botNick)
                && creds.TryGetValue("ChannelName", out channelName)
                && creds.TryGetValue("OsuNick", out osuNick)
                && creds.TryGetValue("AnnounceChannelID", out announceChanID)
                && creds.TryGetValue("DiscordToken", out discordToken)
                && creds.TryGetValue("TwitchAPI", out twitchAPItoken)
                && creds.TryGetValue("TwitchIRC", out twitchIRCtoken)
                && creds.TryGetValue("osuIRC", out osuIRCtoken)
                && creds.TryGetValue("osuAPI", out osuAPIToken);
        }

        private static void getMissingCredentials(Dictionary<string, string> creds)
        {
            Dictionary<string, string> required = new Dictionary<string, string>
            {
                { "BotNick", "" },
                { "ChannelName", "" },
                { "OsuNick", "" },
                { "AnnounceChannelID", "" },
                { "DiscordToken", "" },
                { "TwitchAPI", "" },
                { "TwitchIRC", "" },
                { "osuIRC", "" },
                { "osuAPI", "" },
            };
            
            Console.WriteLine("Hey! You've not completed all the steps to make the bot work. Let's do that shall we?");
            Thread.Sleep(3000);

            var missing = required.Keys.Except(creds.Keys).ToList();
            Type t = typeof(calledudeBot);
            foreach (string s in missing)
            {
                MethodInfo m = t.GetMethod("get" + s, BindingFlags.Static | BindingFlags.NonPublic);
                m.Invoke(null, new object[] { creds });
            }

            File.Create(credFile).Close();
            foreach (KeyValuePair<string, string> k in creds)
            {
                File.AppendAllText(credFile, k.Key + " " + k.Value + Environment.NewLine);
            }

            Console.WriteLine("Alright! We're all done. Let's go! :)");
            Thread.Sleep(3000);
            Console.Clear();
        }

        private static void getCredentials()
        {
            Dictionary<string, string> creds = new Dictionary<string, string>();
            if (File.Exists(credFile))
            {
                creds = File.ReadAllLines(credFile)
                    .Distinct()
                    .Where(p => p.Trim().Split(' ').Length == 2)
                    .ToDictionary(key => key.Split(' ')[0], val => val.Split(' ')[1]);
            }
            while(!tryLoadCredentials(creds))
            {
                getMissingCredentials(creds);
                getCredentials(); //Lets try the newly entered credentials again.
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
    }
}