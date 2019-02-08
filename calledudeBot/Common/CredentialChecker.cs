using calledudeBot.Bots;
using Discord.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot
{
    public static class CredentialChecker
    {
        private static Dictionary<string, string> creds;
        private static readonly string credFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\credentials";
        private static string discordToken, twitchIRCtoken, osuIRCtoken, osuAPIToken, botNick, 
                                            channelName, osuNick, announceChanID, streamerID;
        private static bool verifiedBots;
        public static void ProduceBots()
        {
            getCredentials(); //Makes sure all credentials are at the very least present.

            Bot.testRun = true;
            while (!VerifyCredentials())
            {
                getMissingCredentials();
                tryLoadCredentials();
            }
            Bot.testRun = false;
        }

        public static List<Bot> GetVerifiedBots(out DiscordBot discordBot, out TwitchBot twitchBot, out OsuBot osuBot)
        {
            if (!verifiedBots) throw new InvalidOperationException("You're not allowed to call this method before all bots have been verified.");
            discordBot = new DiscordBot(discordToken, ulong.Parse(announceChanID), ulong.Parse(streamerID));
            osuBot = new OsuBot(osuIRCtoken, osuNick);
            twitchBot = new TwitchBot(twitchIRCtoken, osuAPIToken, osuNick, botNick, channelName);
            return new List<Bot> { discordBot, twitchBot, osuBot };
        }

        //Returns a boolean after running every single bot through the verify function
        private static bool VerifyCredentials()
        {
            bool discToken = false, twitchToken = false, osuToken = false;
            Parallel.Invoke(
                () => discToken = VerifyBot<DiscordBot>(),
                () => twitchToken = VerifyBot<TwitchBot>(),
                () => osuToken = VerifyBot<OsuBot>());
            
            return verifiedBots = discToken && twitchToken && osuToken;
        }

        private static bool VerifyBot<T>() where T : Bot
        {
            using (Bot bot = getBotInstance<T>())
            {
                if (bot == null) return false;
                bool success = false;
                try
                {
                    bot.Start().GetAwaiter().GetResult();
                    success = true; //Will only be set if bot.Start() does not throw an exception
                }
                catch (HttpException)
                {
                    bot.TryLog("Invalid Discord token.");
                    creds.Remove("DiscordToken");
                }
                catch (InvalidOrWrongTokenException)
                {
                    if (bot is OsuBot)
                    {
                        creds.Remove("osuIRC");
                        creds.Remove("OsuNick");
                        bot.TryLog("Invalid token and/or osu!-nickname");
                    }
                    else
                    {
                        creds.Remove("TwitchIRC");
                        bot.TryLog("Invalid token");
                    }
                }
                catch (WebException)
                {
                    bot.TryLog("Invalid osu! API token.");
                    creds.Remove("osuAPI");
                }
                catch (ArgumentException)
                {
                    creds.Remove("OsuNick");
                    bot.TryLog("Invalid osu!-nickname.");
                }
                finally
                {
                    bot.TryLog($"Verification {(success ? "SUCCESS." : "FAIL.")}");
                    bot.Logout().GetAwaiter().GetResult();
                }
                return success;
            }
        }

        private static Bot getBotInstance<T>() where T : Bot
        {
            if (typeof(T) == typeof(DiscordBot))
            {
                if (!ulong.TryParse(announceChanID, out var _announceChanID) && !ulong.TryParse(streamerID, out var _streamerID))
                {
                    creds.Remove("AnnounceChannelID");
                    creds.Remove("StreamerID");
                }
                if (!ulong.TryParse(announceChanID, out _announceChanID))
                {
                    creds.Remove("AnnounceChannelID");
                }
                if (!ulong.TryParse(streamerID, out _streamerID))
                {
                    creds.Remove("StreamerID");
                }
                return new DiscordBot(discordToken, _announceChanID, _streamerID);
            }
            else if (typeof(T) == typeof(TwitchBot))
            {
                return new TwitchBot(twitchIRCtoken, osuAPIToken, osuNick, botNick, channelName);
            }
            else
            {
                return new OsuBot(osuIRCtoken, osuNick);
            }
        }

        private static bool tryLoadCredentials()
        {
            return creds.TryGetValue("BotNick", out botNick)
                && creds.TryGetValue("ChannelName", out channelName)
                && creds.TryGetValue("OsuNick", out osuNick)
                && creds.TryGetValue("AnnounceChannelID", out announceChanID)
                && creds.TryGetValue("DiscordToken", out discordToken)
                && creds.TryGetValue("TwitchIRC", out twitchIRCtoken)
                && creds.TryGetValue("osuIRC", out osuIRCtoken)
                && creds.TryGetValue("osuAPI", out osuAPIToken)
                && creds.TryGetValue("StreamerID", out streamerID);
        }

        private static void getMissingCredentials()
        {
            Dictionary<string, string> required = new Dictionary<string, string>
            {
                { "BotNick", "" },
                { "ChannelName", "" },
                { "OsuNick", "" },
                { "AnnounceChannelID", "" },
                { "DiscordToken", "" },
                { "TwitchIRC", "" },
                { "osuIRC", "" },
                { "osuAPI", "" },
                { "StreamerID", "" }
            };

            Console.WriteLine("Hey! You've not completed all the steps to make the bot work. Let's do that shall we?");
            Thread.Sleep(3000);

            var missing = required.Keys.Except(creds.Keys).ToList();
            Type t = typeof(CredentialChecker);
            foreach (string s in missing)
            {
                MethodInfo m = t.GetMethod("get" + s, BindingFlags.NonPublic | BindingFlags.Static);
                m.Invoke(null, null);
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
            if (File.Exists(credFile))
            {
                creds = File.ReadAllLines(credFile)
                    .Distinct()
                    .Where(p => p.Trim().Split(' ').Length == 2)
                    .ToDictionary(key => key.Split(' ')[0], val => val.Split(' ')[1]);
            }
            else
            {
                File.Create(credFile).Close();
                creds = new Dictionary<string, string>();
            }
            while (!tryLoadCredentials())
            {
                getMissingCredentials();
                getCredentials(); //Lets try the newly entered credentials again.
            }
        }

        #region Setup
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
            ConsoleKey c;
            do c = Console.ReadKey(true).Key;
            while (c != ConsoleKey.Y && c != ConsoleKey.N);
            Console.WriteLine();

            return c == ConsoleKey.Y;
        }

        private static void getBotNick()
        {
            Console.Write("What will the username be of your bot be?: ");
            botNick = Console.ReadLine();
            creds.Add("BotNick", botNick);
        }

        private static void getChannelName()
        {
            Console.Write("Will you be using your personal twitch account as the 'bot'? Y/N: ");
            if (getConfirmation())
            {
                channelName = "#" + botNick;
            }
            else
            {
                Console.Write("Please enter your nickname on twitch: ");
                channelName = "#" + Console.ReadLine();
            }
            creds.Add("ChannelName", channelName);
        }

        private static void getOsuNick()
        {
            Console.Write("What's your osu! username?: ");
            osuNick = Console.ReadLine();
            creds.Add("OsuNick", osuNick);
        }

        private static void getAnnounceChannelID()
        {
            Console.Write("What channel on your discord server do you want the announcements to be made on? (Long number): ");
            openWebsite(4000, "https://support.discordapp.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID");

            announceChanID = Console.ReadLine();
            creds.Add("AnnounceChannelID", announceChanID);
        }

        private static void getDiscordToken()
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

        private static void getTwitchIRC()
        {
            Console.Write("Twitch IRC token: ");
            openWebsite(2000, "http://www.twitchapps.com/tmi/");
            twitchIRCtoken = Console.ReadLine();
            creds.Add("TwitchIRC", twitchIRCtoken);
        }

        private static void getosuIRC()
        {
            Console.Write("osu! IRC password: ");
            openWebsite(2000, "https://osu.ppy.sh/p/irc");
            osuIRCtoken = Console.ReadLine();
            creds.Add("osuIRC", osuIRCtoken);
        }

        private static void getosuAPI()
        {
            Console.Write("osu! API key: ");
            openWebsite(2000, "https://osu.ppy.sh/p/api");
            osuAPIToken = Console.ReadLine();
            creds.Add("osuAPI", osuAPIToken);
        }

        private static void getStreamerID()
        {
            Console.Write("YOUR User-ID (On Discord): ");
            streamerID = Console.ReadLine();
            creds.Add("StreamerID", streamerID);
        }

        #endregion

    }
}
