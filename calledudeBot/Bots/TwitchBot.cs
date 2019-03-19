using calledudeBot.Chat;
using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Bots
{
    public sealed class TwitchBot : IrcClient
    {
        private readonly RelayHandler messageHandler;
        private List<string> mods = new List<string>();
        private Timer modLockTimer;
        private bool modCheckLock;
        private OsuUser oldOsuData;
        private APIHandler<OsuUser> api;
        private readonly string osuAPIToken, osuNick;

        public TwitchBot(string token, string osuAPIToken, string osuNick, string botNick, string channelName)
            : base("irc.chat.twitch.tv", "Twitch", 366)
        {
            Token = token;
            this.osuAPIToken = osuAPIToken;
            this.osuNick = osuNick;
            this.channelName = channelName;

            nick = botNick;
            messageHandler = new RelayHandler(this, channelName, osuAPIToken);
            OnReady += onReady;
        }

        private async Task onReady()
        {
            modLockTimer = new Timer(60000);
            modLockTimer.Elapsed += modLockEvent;
            modLockTimer.Start();
            await WriteLine("CAP REQ :twitch.tv/commands");

            GetMods();
            api = new APIHandler<OsuUser>($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}");
            api.DataReceived += checkUserUpdate;
            await api.Start();
        }

        protected override async Task Listen()
        {
            while (true)
            {
                var buffer = await input.ReadLineAsync();
                var b = buffer.Split(' ');
                if (b[1] == "PRIVMSG") //This is a private message, check if we should respond to it.
                {
                    messageHandler.DetermineResponse(new IrcMessage(buffer));
                }
                else if (buffer.StartsWith($":tmi.twitch.tv NOTICE {channelName} :The moderators of this channel are:"))
                {
                    int modsIndex = buffer.LastIndexOf(':') + 1;
                    var modsArr = buffer.Substring(modsIndex).Split(',');
                    mods = modsArr.Select(x => x.Trim()).ToList();
                }
                else if (b[0] == "PING")
                {
                    await SendPong(buffer);
                }
            }
        }

        private void checkUserUpdate(OsuUser user)
        {
            if (user == null) throw new ArgumentException("Invalid username.", nameof(user));

            if (oldOsuData != null && oldOsuData.Rank != user.Rank && Math.Abs(user.PP - oldOsuData.PP) >= 0.1)
            {
                int rankDiff = user.Rank - oldOsuData.Rank;
                float ppDiff = user.PP - oldOsuData.PP;

                string formatted = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", Math.Abs(ppDiff));
                string totalPP = user.PP.ToString(CultureInfo.InvariantCulture);

                string rankMessage = $"{Math.Abs(rankDiff)} ranks (#{user.Rank}). ";
                string ppMessage = $"PP: {(ppDiff < 0 ? "-" : "+")}{formatted}pp ({totalPP}pp)";
                SendMessage(new IrcMessage($"{user.Username} just {(rankDiff < 0 ? "gained" : "lost")} {rankMessage} {ppMessage}"));
            }
            oldOsuData = user;
        }

        private void modLockEvent(object sender, ElapsedEventArgs e)
        {
            modCheckLock = false;
            modLockTimer.Stop();
        }

        public List<string> GetMods()
        {
            if (!modCheckLock)
            {
                WriteLine($"PRIVMSG {channelName} /mods").GetAwaiter().GetResult();
                modCheckLock = true;
                modLockTimer.Start();
            }
            return mods;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            messageHandler.Dispose();
            api?.Dispose();
            modLockTimer?.Dispose();
        }
    }
}