using calledudeBot.Chat;
using calledudeBot.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Nito.AsyncEx;
using System;

namespace calledudeBot.Bots
{
    public sealed class TwitchBot : IrcClient
    {
        private List<string> mods;
        private OsuUserService osuUserService;
        private RelayHandler messageHandler;
        private Timer modLockTimer;
        private bool modCheckLock;
        private readonly AsyncAutoResetEvent modWait = new AsyncAutoResetEvent();
        private readonly string _osuAPIToken, _osuNick, _botNick;
        private readonly OsuBot _osuBot;

        public TwitchBot(string token, string osuAPIToken, string osuNick, string botNick, string channelName, OsuBot osuBot)
            : base("irc.chat.twitch.tv", "Twitch", 366)
        {
            Token = token;
            this.channelName = channelName;
            nick = botNick;

            _osuAPIToken = osuAPIToken;
            _osuNick = osuNick;
            _botNick = botNick;
            _osuBot = osuBot;

            Ready += OnReady;
            MessageReceived += HandleMessage;
            UnhandledMessage += HandleRawMessage;
        }

        private async Task OnReady()
        {
            modLockTimer = new Timer(60000);
            messageHandler = new RelayHandler(this, channelName, _osuAPIToken, _osuBot);
            osuUserService = new OsuUserService(_osuAPIToken, _osuNick, this);
            modLockTimer.Elapsed += modLockEvent;
            modLockTimer.Start();

            await WriteLine("CAP REQ :twitch.tv/commands");
            await osuUserService.Start();
        }

        private async void HandleMessage(string message, string user)
        {
            var mods = await GetMods();
            var isMod = mods.Any(u => u.Equals(user, StringComparison.OrdinalIgnoreCase));

            var sender = new User(user, isMod);
            var msg = new IrcMessage(message, sender);

            await messageHandler.DetermineResponse(msg);
        }

        private void HandleRawMessage(string buffer)
        {
            if (buffer.Contains("The moderators of this channel are:"))
            {
                int modsIndex = buffer.LastIndexOf(':') + 1;
                var modsArr = buffer.Substring(modsIndex).Split(',');
                mods = modsArr.Select(x => x.Trim()).ToList();
                modWait.Set();
            }
        }

        private void modLockEvent(object sender, ElapsedEventArgs e)
        {
            modCheckLock = false;
            modLockTimer.Stop();
        }

        private async Task<List<string>> GetMods()
        {
            if (!modCheckLock)
            {
                modCheckLock = true;
                modLockTimer.Start();
                await WriteLine($"PRIVMSG {channelName} /mods");
                await modWait.WaitAsync();
            }

            return mods;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            messageHandler?.Dispose();
            osuUserService?.Dispose();
            modLockTimer?.Dispose();
        }
    }
}