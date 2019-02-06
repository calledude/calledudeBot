using System;
using System.Collections.Generic;
using calledudeBot.Chat;
using calledudeBot.Services;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;

namespace calledudeBot.Bots
{
    public sealed class TwitchBot : IrcClient
    {
        private readonly RelayHandler<IrcMessage> messageHandler;
        private List<string> mods = new List<string>();
        private Timer modLockTimer;
        private bool modCheckLock;
        private OsuUser oldOsuData;
        private readonly APIHandler<OsuUser> api;

        public TwitchBot(string token, string osuAPIToken, string osuNick, string botNick, string channelName) 
            : base("irc.chat.twitch.tv", "Twitch")
        {
            Token = token;
            this.channelName = channelName;

            nick = botNick;
            messageHandler = new RelayHandler<IrcMessage>(this, channelName, osuAPIToken);

            api = new APIHandler<OsuUser>($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}");
            api.DataReceived += checkUserUpdate;
        }

        internal async override Task Start()
        {
            modLockTimer = new Timer(60000);
            modLockTimer.Elapsed += modLockEvent;
            modLockTimer.Start();
            api.Start();
            WriteLine("CAP REQ :twitch.tv/commands");
            await base.Start();
        }

        public override async void Listen()
        {
            while(true)
            {
                var buf = await input.ReadLineAsync();
                var b = buf.Split(' ');
                if (b[1] == "PRIVMSG") //This is a private message, check if we should respond to it.
                {
                    messageHandler.DetermineResponse(new IrcMessage(buf));
                }
                else if (buf.StartsWith($":tmi.twitch.tv NOTICE {channelName} :The moderators of this channel are:"))
                {
                    int modsIndex = buf.LastIndexOf(':') + 1;
                    var modsArr = buf.Substring(modsIndex).Split(',');
                    mods = modsArr.Select(x => x.Trim()).ToList();
                }
                else if (b[0] == "PING")
                {
                    SendPong();
                }
            }
        }

        private void checkUserUpdate(OsuUser user)
        {
            if(user == null) throw new ArgumentException("Invalid username.", nameof(user));

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
                WriteLine($"PRIVMSG {channelName} /mods");
                modCheckLock = true;
                modLockTimer.Start();
            }
            return mods;
        }

        protected override void Dispose(bool disposing)
        {
            messageHandler.Dispose();
            base.Dispose(disposing);
            api.Dispose();
            modLockTimer?.Dispose();
        }
    }
}