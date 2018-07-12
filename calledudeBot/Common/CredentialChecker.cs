﻿using calledudeBot.Bots;
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
    public enum TestSubject
    {
        Discord, Osu, Twitch
    }

    public static class CredentialChecker
    {
        private static Dictionary<string, string> creds;
        private static string credFile = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) + @"\credentials";
        private static string discordToken, twitchAPItoken, twitchIRCtoken, osuIRCtoken,
                                osuAPIToken, botNick, channelName, osuNick, announceChanID;

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
            Console.Clear();
        }


        public static List<Bot> GetVerifiedBots(out DiscordBot discordBot, out TwitchBot twitchBot, out OsuBot osuBot)
        {
            discordBot = new DiscordBot(discordToken, twitchAPItoken, channelName, announceChanID);
            osuBot = new OsuBot(osuIRCtoken, osuNick);
            twitchBot = new TwitchBot(twitchIRCtoken, osuAPIToken, osuNick, botNick, channelName);
            return new List<Bot> { discordBot, twitchBot, osuBot };
        }


        //Returns a boolean after running every single bot through the verify function
        private static bool VerifyCredentials()
        {
            return VerifyToken(TestSubject.Discord)
                & VerifyServices(TestSubject.Discord)
                & VerifyToken(TestSubject.Twitch)
                & VerifyServices(TestSubject.Twitch)
                & VerifyToken(TestSubject.Osu);
        }

        private static bool VerifyToken(TestSubject testSubject)
        {
            Bot bot = getBotInstance(testSubject);
            bool success = false;
            try
            {
                Task.Run(() => bot.TryRun())
                    .GetAwaiter().GetResult();
                success = true; //Will only be set if bot.TryRun() does not throw an exception
            }
            catch(HttpException)
            {
                bot.tryLog("Invalid Discord token.");
                creds.Remove("DiscordToken");
            }
            catch (InvalidOrWrongTokenException e)
            {
                if(bot is OsuBot)
                {
                    creds.Remove("osuIRC");
                    creds.Remove("OsuNick");
                }
                else
                {
                    creds.Remove("TwitchIRC");
                }
                bot.tryLog(e.Message);
            }
            catch(ArgumentException)
            {
                creds.Remove("OsuNick");
            }
            finally
            {
                bot.tryLog($"Token: {(success ? "Succeeded." : "Failed.")}");
            }
            return success;
        }

        private static bool VerifyServices(TestSubject testSubject)
        {
            Bot bot = getBotInstance(testSubject);
            bool success = false;
            try
            {
                Task.Run(() => bot.StartServices())
                    .GetAwaiter().GetResult();
                success = true; //Will only be set if bot.StartServices() does not throw an exception
            }
            catch (WebException)
            {
                if (bot is TwitchBot)
                {
                    bot.tryLog("Invalid osu! API token.");
                    creds.Remove("osuAPI");
                }
                else if (bot is DiscordBot)
                {
                    bot.tryLog("Invalid Twitch API token.");
                    creds.Remove("TwitchAPI");
                }
            }
            finally
            {
                bot.tryLog($"Services: {(success ? "Succeeded." : "Failed.")}");
            }
            return success;
        }

        private static Bot getBotInstance(TestSubject testSubject)
        {
            Bot bot;
            if (testSubject is TestSubject.Discord)
            {
                bot = new DiscordBot(discordToken, twitchAPItoken, channelName, announceChanID);
            }
            else if (testSubject is TestSubject.Twitch)
            {
                bot = new TwitchBot(twitchIRCtoken, osuAPIToken, osuNick, botNick, channelName);
            }
            else
            {
                bot = new OsuBot(osuIRCtoken, osuNick);
            }
            return bot;
        }

        private static bool tryLoadCredentials()
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

        private static void getMissingCredentials()
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
            while (!tryLoadCredentials())
            {
                getMissingCredentials();
                getCredentials(); //Lets try the newly entered credentials again.
            }
        }


        //Setup related functions below
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
            if (getConfirmation()) channelName = "#" + botNick;
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

        private static void getTwitchAPI()
        {
            Console.Write("Twitch API token (Called 'Client ID' on twitch): ");
            openWebsite(2000, "https://dev.twitch.tv/dashboard/apps");
            twitchAPItoken = Console.ReadLine();
            creds.Add("TwitchAPI", twitchAPItoken);
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


    }
}
