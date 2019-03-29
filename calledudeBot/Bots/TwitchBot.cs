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
        private readonly OsuUserService osuUserService;
        private readonly RelayHandler messageHandler;
        private readonly Timer modLockTimer;
        private bool modCheckLock;
        private readonly AsyncAutoResetEvent modWait = new AsyncAutoResetEvent();

        public TwitchBot(string token, string osuAPIToken, string osuNick, string botNick, string channelName, OsuBot osuBot)
            : base("irc.chat.twitch.tv", "Twitch", 366)
        {
            Token = token;
            this.channelName = channelName;
            nick = botNick;

            modLockTimer = new Timer(60000);
            messageHandler = new RelayHandler(this, channelName, osuAPIToken, osuBot);
            osuUserService = new OsuUserService(osuAPIToken, osuNick, this);

            Ready += OnReady;
            MessageReceived += HandleMessage;
            UnhandledMessage += HandleRawMessage;
        }

        private async Task OnReady()
        {
            IrcMessage.TwitchBot = this;

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
            messageHandler.Dispose();
            osuUserService?.Dispose();
            modLockTimer?.Dispose();
        }
    }
}