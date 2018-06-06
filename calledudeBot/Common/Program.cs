using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using calledudeBot.Bots;

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
        private static Thread osuThread = new Thread(Osu);
        private static Thread discordThread = new Thread(Discord);
        private static Thread twitchThread = new Thread(Twitch);
        private static Thread hookyThread = new Thread(Hooky);

        private static string discordToken, twitchAPItoken, twitchIRCtoken, osuIRCtoken;
        public static string osuAPIToken, botNick, channelName, osuNick;
        public static ulong discordGeneralID;

        private static void Main(string[] args)
        {
            Console.Title = "calledudeBot";
            initialSetup();
            getCredentials();
            Clean();

            discordBot = new DiscordBot();
            osuBot = new OsuBot();
            twitchBot = new TwitchBot();
            hooky = new Hooky(twitchBot);

            discordThread.Start();
            osuThread.Start();
            twitchThread.Start();
            hookyThread.Start();
        }

        private static void getCredentials()
        {
            var credArr = File.ReadAllLines(credFile);
            List<string> credList = new List<string>(credArr);
            foreach (string s in credList)
            {
                if (s.StartsWith("BotNick")) botNick = s.Split(' ')[1];
                else if (s.StartsWith("ChannelName")) channelName = s.Split(' ')[1];
                else if (s.StartsWith("OsuNick")) osuNick = s.Split(' ')[1];
                else if (s.StartsWith("DiscordGeneralID")) discordGeneralID = Convert.ToUInt64(s.Split(' ')[1]);
                else if (s.StartsWith("DiscordToken")) discordToken = s.Split(' ')[1];
                else if (s.StartsWith("TwitchAPI")) twitchAPItoken = s.Split(' ')[1];
                else if (s.StartsWith("TwitchIRC")) twitchIRCtoken = s.Split(' ')[1];
                else if (s.StartsWith("osuIRC")) osuIRCtoken = s.Split(' ')[1];
                else if (s.StartsWith("osuAPI")) osuAPIToken = s.Split(' ')[1];
            }
            
        }

        private static void initialSetup()
        {
            if(!File.Exists(credFile)) File.Create(credFile).Close();
            var credArr = File.ReadAllLines(credFile);
            List<string> credList = new List<string>(credArr);

            if (credList.Count == 9) return;
            credList = new List<string>();

            Console.WriteLine("Hey! I see you've not yet gone through the necessary steps to make the bot work. Let's do that shall we?");
            Thread.Sleep(5000);

            Console.Write("What will the username be of your bot be?: ");
            botNick = Console.ReadLine();
            credList.Add("BotNick " + botNick);

            Console.Write("Will you be using your personal twitch account as the 'bot'? Y/N: ");
            ConsoleKey c = ConsoleKey.A;
            while (c != ConsoleKey.Y && c != ConsoleKey.N)
            {
                c = Console.ReadKey(false).Key;
            }
            Console.WriteLine();
            if (c == ConsoleKey.Y) channelName = "#" + botNick;
            else
            {
                Console.Write("Please enter your nickname on twitch: ");
                channelName = "#" + Console.ReadLine();
            }

            credList.Add("ChannelName " + channelName);

            Console.Write("Ok then! What's your osu! username?: ");
            osuNick = Console.ReadLine();
            credList.Add("OsuNick " + osuNick);
           
            Console.WriteLine("Alright, done with the annoying nickname questions. Now on to the good stuff! :)");
            Console.WriteLine("Now, this is where stuff starts to get a bit tricky. I'll help you out. (A bit)");
            Console.Write("What channel on your discord server do you want the announcements to be made on? (Long number): ");
            Thread.Sleep(4000);
            Process.Start("https://support.discordapp.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID");
            discordGeneralID = Convert.ToUInt64(Console.ReadLine());
            credList.Add("DiscordGeneralID " + discordGeneralID);

            Console.WriteLine("First you need to create a bot/app on the discord developer website.");
            Console.Write("Discord token: ");
            Thread.Sleep(3000);
            Process.Start("https://discordapp.com/developers/applications/me");
            discordToken = Console.ReadLine();
            credList.Add("DiscordToken " + discordToken);

            //https://discordapp.com/oauth2/authorize?client_id=201013813019934721&scope=bot
            Console.WriteLine("Have you figured out how to get the bot to join your own channel? Y/N");
            ConsoleKey ck = ConsoleKey.A;
            while (ck != ConsoleKey.Y && ck != ConsoleKey.N)
            {
                ck = Console.ReadKey(false).Key;
            }
            Console.WriteLine();
            if (ck == ConsoleKey.N)
            {
                Console.Write("Alright, enter the bots Client-ID here: ");
                Process.Start($"https://discordapp.com/oauth2/authorize?client_id={Console.ReadLine()}&scope=bot");
            }

            Console.Write("Twitch API token (Called 'Client ID' on twitch): ");
            Thread.Sleep(2000);
            Process.Start("https://dev.twitch.tv/dashboard/apps");
            twitchAPItoken = Console.ReadLine();
            credList.Add("TwitchAPI " + twitchAPItoken);

            Console.Write("Twitch IRC token: ");
            Thread.Sleep(2000);
            Process.Start("http://www.twitchapps.com/tmi/");
            twitchIRCtoken = Console.ReadLine();
            credList.Add("TwitchIRC " + twitchIRCtoken);

            Console.Write("osu! IRC password: ");
            Thread.Sleep(2000);
            Process.Start("https://osu.ppy.sh/p/irc");
            osuIRCtoken = Console.ReadLine();
            credList.Add("osuIRC " + osuIRCtoken);

            Console.Write("osu! API key: ");
            Thread.Sleep(2000);
            Process.Start("https://osu.ppy.sh/p/api");
            osuAPIToken = Console.ReadLine();
            credList.Add("osuAPI " + osuAPIToken);

            Console.WriteLine("Alright! We're all done. Let's go! :)");
            Thread.Sleep(3000);

            File.WriteAllLines(credFile, credList);
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

            foreach (string line in cleanUpArr)
            {
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
            hooky.Start();
        }
    }
}