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
        private static Dictionary<string, string> _creds;
        private static readonly string _credFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\credentials";
        private static string _discordToken, _twitchIRCtoken, _osuIRCtoken, _osuAPIToken, _botNick,
                                            _channelName, _osuNick, _announceChanID, _streamerID;
        private static bool _verifiedBots;

        public static async Task ProduceBots()
        {
            GetCredentials(); //Makes sure all credentials are at the very least present.

            Bot.TestRun = true;
            while (!await VerifyCredentials())
            {
                getMissingCredentials();
                TryLoadCredentials();
            }
            Bot.TestRun = false;
        }

        public static Bot[] GetVerifiedBots(out DiscordBot discordBot, out TwitchBot twitchBot, out OsuBot osuBot)
        {
            if (!_verifiedBots) throw new InvalidOperationException("You're not allowed to call this method before all bots have been verified.");
            discordBot = new DiscordBot(_discordToken, ulong.Parse(_announceChanID), ulong.Parse(_streamerID));
            osuBot = new OsuBot(_osuIRCtoken, _osuNick);
            twitchBot = new TwitchBot(_twitchIRCtoken, _osuAPIToken, _osuNick, _botNick, _channelName, osuBot);
            return new Bot[] { discordBot, twitchBot, osuBot };
        }

        //Returns a boolean after running every single bot through the verify function
        private static async Task<bool> VerifyCredentials()
        {
            var discToken = VerifyBot<DiscordBot>();
            var twitchToken = VerifyBot<TwitchBot>();
            var osuToken = VerifyBot<OsuBot>();

            return _verifiedBots = await osuToken && await twitchToken && await discToken;
        }

        public static async Task<bool> VerifyBot<T>() where T : Bot
        {
            using (Bot bot = getBotInstance<T>())
            {
                bool success = false;
                try
                {
                    await bot.Start();
                    success = true; //Will only be set if bot.Start() does not throw an exception
                }
                catch (HttpException)
                {
                    bot.TryLog("Invalid Discord token.");
                    _creds.Remove("DiscordToken");
                }
                catch (InvalidOrWrongTokenException)
                {
                    if (bot is OsuBot)
                    {
                        _creds.Remove("osuIRC");
                        _creds.Remove("OsuNick");
                        bot.TryLog("Invalid token and/or osu!-nickname");
                    }
                    else
                    {
                        _creds.Remove("TwitchIRC");
                        bot.TryLog("Invalid token");
                    }
                }
                catch (WebException)
                {
                    bot.TryLog("Invalid osu! API token.");
                    _creds.Remove("osuAPI");
                }
                catch (ArgumentException)
                {
                    _creds.Remove("OsuNick");
                    bot.TryLog("Invalid osu!-nickname.");
                }
                finally
                {
                    bot.TryLog($"Verification {(success ? "SUCCESS." : "FAIL.")}");
                    await bot.Logout();
                }
                return success;
            }
        }

        private static Bot getBotInstance<T>() where T : Bot
        {
            if (typeof(T) == typeof(DiscordBot))
            {
                if (!ulong.TryParse(_announceChanID, out var announceChanID) && !ulong.TryParse(_streamerID, out var streamerID))
                {
                    _creds.Remove("AnnounceChannelID");
                    _creds.Remove("StreamerID");
                }
                if (!ulong.TryParse(_announceChanID, out announceChanID))
                {
                    _creds.Remove("AnnounceChannelID");
                }
                if (!ulong.TryParse(_streamerID, out streamerID))
                {
                    _creds.Remove("StreamerID");
                }
                return new DiscordBot(_discordToken, announceChanID, streamerID);
            }
            else if (typeof(T) == typeof(TwitchBot))
            {
                return new TwitchBot(_twitchIRCtoken, _osuAPIToken, _osuNick, _botNick.ToLower(), _channelName.ToLower(), null);
            }
            else
            {
                return new OsuBot(_osuIRCtoken, _osuNick);
            }
        }

        public static bool TryLoadCredentials()
        {
            return _creds.TryGetValue("BotNick", out _botNick)
                && _creds.TryGetValue("ChannelName", out _channelName)
                && _creds.TryGetValue("OsuNick", out _osuNick)
                && _creds.TryGetValue("AnnounceChannelID", out _announceChanID)
                && _creds.TryGetValue("DiscordToken", out _discordToken)
                && _creds.TryGetValue("TwitchIRC", out _twitchIRCtoken)
                && _creds.TryGetValue("osuIRC", out _osuIRCtoken)
                && _creds.TryGetValue("osuAPI", out _osuAPIToken)
                && _creds.TryGetValue("StreamerID", out _streamerID);
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

            var missing = required.Keys.Except(_creds.Keys).ToList();
            Type t = typeof(CredentialChecker);
            foreach (string s in missing)
            {
                MethodInfo m = t.GetMethod("get" + s, BindingFlags.NonPublic | BindingFlags.Static);
                m.Invoke(null, null);
            }

            File.Create(_credFile).Close();
            foreach (KeyValuePair<string, string> k in _creds)
            {
                File.AppendAllText(_credFile, k.Key + " " + k.Value + Environment.NewLine);
            }

            Console.WriteLine("Alright! We're all done. Let's go! :)");
            Thread.Sleep(3000);
            Console.Clear();
        }

        public static void GetCredentials()
        {
            if (File.Exists(_credFile))
            {
                _creds = File.ReadAllLines(_credFile)
                    .Distinct()
                    .Select(x => x.Trim().Split(' '))
                    .Where(p => p.Length == 2)
                    .ToDictionary(key => key[0], val => val[1]);
            }
            else
            {
                File.Create(_credFile).Close();
                _creds = new Dictionary<string, string>();
            }
            while (!TryLoadCredentials())
            {
                getMissingCredentials();
                GetCredentials(); //Lets try the newly entered credentials again.
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
            _botNick = Console.ReadLine();
            _creds.Add("BotNick", _botNick);
        }

        private static void getChannelName()
        {
            Console.Write("Will you be using your personal twitch account as the 'bot'? Y/N: ");
            if (getConfirmation())
            {
                _channelName = "#" + _botNick;
            }
            else
            {
                Console.Write("Please enter your nickname on twitch: ");
                _channelName = "#" + Console.ReadLine();
            }
            _creds.Add("ChannelName", _channelName);
        }

        private static void getOsuNick()
        {
            Console.Write("What's your osu! username?: ");
            _osuNick = Console.ReadLine();
            _creds.Add("OsuNick", _osuNick);
        }

        private static void getAnnounceChannelID()
        {
            Console.Write("What channel on your discord server do you want the announcements to be made on? (Long number): ");
            openWebsite(4000, "https://support.discordapp.com/hc/en-us/articles/206346498-Where-can-I-find-my-User-Server-Message-ID");

            _announceChanID = Console.ReadLine();
            _creds.Add("AnnounceChannelID", _announceChanID);
        }

        private static void getDiscordToken()
        {
            Console.WriteLine("First you need to create a bot/app on the discord developer website.");
            Console.Write("Discord token: ");
            openWebsite(3000, "https://discordapp.com/developers/applications/me");

            _discordToken = Console.ReadLine();
            Console.Write("Have you figured out how to get the bot to join your own channel? Y/N: ");

            if (!getConfirmation())
            {
                Console.Write("Alright, enter the bots Client-ID here: ");
                openWebsite(0, $"https://discordapp.com/oauth2/authorize?client_id={Console.ReadLine()}&scope=bot");
            }
            _creds.Add("DiscordToken", _discordToken);
        }

        private static void getTwitchIRC()
        {
            Console.Write("Twitch IRC token: ");
            openWebsite(2000, "http://www.twitchapps.com/tmi/");
            _twitchIRCtoken = Console.ReadLine();
            _creds.Add("TwitchIRC", _twitchIRCtoken);
        }

        private static void getosuIRC()
        {
            Console.Write("osu! IRC password: ");
            openWebsite(2000, "https://osu.ppy.sh/p/irc");
            _osuIRCtoken = Console.ReadLine();
            _creds.Add("osuIRC", _osuIRCtoken);
        }

        private static void getosuAPI()
        {
            Console.Write("osu! API key: ");
            openWebsite(2000, "https://osu.ppy.sh/p/api");
            _osuAPIToken = Console.ReadLine();
            _creds.Add("osuAPI", _osuAPIToken);
        }

        private static void getStreamerID()
        {
            Console.Write("YOUR User-ID (On Discord): ");
            _streamerID = Console.ReadLine();
            _creds.Add("StreamerID", _streamerID);
        }

        #endregion

    }
}
