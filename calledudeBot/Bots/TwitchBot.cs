using System;
using System.Collections.Generic;
using calledudeBot.Chat;
using calledudeBot.Services;
using System.Timers;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public class TwitchBot : IrcClient
    {
        private MessageHandler messageHandler;
        private List<string> mods = new List<string>();
        private Timer modLockTimer;
        private bool modCheckLock;
        private OsuUserData oldOsuData;

        public TwitchBot(string token, string osuAPIToken, string osuNick, string botNick, string channelName) 
            : base("irc.chat.twitch.tv", "Twitch")
        {
            Token = token;
            this.channelName = channelName;

            nick = botNick;
            messageHandler = new MessageHandler(this, channelName, osuAPIToken);

            Api = new APIHandler($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}", RequestData.OsuUser);
            Api.DataReceived += checkUserUpdate;
        }

        internal async override Task Start()
        {
            modLockTimer = new Timer(60000);
            modLockTimer.Elapsed += modLockEvent;
            modLockTimer.Start();

            WriteLine("CAP REQ :twitch.tv/commands");
            await base.Start();
        }

        public override void Listen()
        {
            for (buf = input.ReadLine(); ; buf = input.ReadLine())
            {
                var b = buf.Split(' ');
                if (b[1] == "PRIVMSG") //This is a private message, check if we should respond to it.
                {
                    Message message = new Message(buf, this);
                    messageHandler.determineResponse(message);
                }
                else if (buf.StartsWith($":tmi.twitch.tv NOTICE {channelName} :The moderators of this channel are:"))
                {
                    int modsIndex = buf.LastIndexOf(':') + 1;
                    var modsArr = buf.Substring(modsIndex).Split(',');
                    mods = modsArr.Select(x => x.Trim()).ToList();
                }
                else if (b[0] == "PING")
                {
                    string pong = buf.Replace("PING", "PONG");
                    WriteLine(pong);
                    tryLog(pong);
                }
            }
        }

        private void checkUserUpdate(JsonData jsonData)
        {
            if (jsonData.osuUserData.Count == 0) throw new ArgumentException("Invalid username.", "jsonData.osuUserData");

            OsuUserData newOsuData = jsonData?.osuUserData[0];
            if (oldOsuData != null && newOsuData != null)
            {
                if (oldOsuData.pp_rank != newOsuData.pp_rank && Math.Abs(newOsuData.pp_raw - oldOsuData.pp_raw) >= 0.1)
                {
                    int rankDiff = newOsuData.pp_rank - oldOsuData.pp_rank;
                    float ppDiff = newOsuData.pp_raw - oldOsuData.pp_raw;

                    string formatted = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.00}", Math.Abs(ppDiff));
                    string totalPP = newOsuData.pp_raw.ToString(System.Globalization.CultureInfo.InvariantCulture);

                    string rankMessage = $"{Math.Abs(rankDiff)} ranks (#{newOsuData.pp_rank}). ";
                    string ppMessage = $"PP: {(ppDiff < 0 ? "-" : "+")}{formatted}pp ({totalPP}pp)";
                    sendMessage(new Message($"{newOsuData.username} just {(rankDiff < 0 ? "gained" : "lost")} {rankMessage} {ppMessage}"));
                }
            }
            oldOsuData = newOsuData;
        }

        private void modLockEvent(object sender, ElapsedEventArgs e)
        {
            modCheckLock = false;
            modLockTimer.Stop();
        }
        
        public List<string> getMods()
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
            base.Dispose(disposing);
            modLockTimer?.Dispose();
        }
    }
}