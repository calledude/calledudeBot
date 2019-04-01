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
        private List<string> _mods;
        private OsuUserService _osuUserService;
        private RelayHandler _messageHandler;
        private Timer _modLockTimer;
        private bool _modCheckLock;
        private readonly AsyncAutoResetEvent _modWait;
        private readonly string _osuAPIToken, _osuNick, _botNick;
        private readonly OsuBot _osuBot;

        public TwitchBot(string token, string osuAPIToken, string osuNick, string botNick, string channelName, OsuBot osuBot)
            : base("irc.chat.twitch.tv", "Twitch", 366)
        {
            Token = token;
            ChannelName = channelName;
            Nick = botNick;

            _osuAPIToken = osuAPIToken;
            _osuNick = osuNick;
            _botNick = botNick;
            _osuBot = osuBot;

            _modWait = new AsyncAutoResetEvent();

            Ready += OnReady;
            MessageReceived += HandleMessage;
            UnhandledMessage += HandleRawMessage;
        }

        private async Task OnReady()
        {
            _modLockTimer = new Timer(60000);
            _messageHandler = new RelayHandler(this, ChannelName, _osuAPIToken, _osuBot);
            _osuUserService = new OsuUserService(_osuAPIToken, _osuNick, this);
            _modLockTimer.Elapsed += ModLockEvent;
            _modLockTimer.Start();

            await WriteLine("CAP REQ :twitch.tv/commands");
            await _osuUserService.Start();
        }

        private async void HandleMessage(string message, string user)
        {
            var mods = await GetMods();
            var isMod = mods.Any(u => u.Equals(user, StringComparison.OrdinalIgnoreCase));

            var sender = new User(user, isMod);
            var msg = new IrcMessage(message, sender);

            await _messageHandler.DetermineResponse(msg);
        }

        private void HandleRawMessage(string buffer)
        {
            if (buffer.Contains("The moderators of this channel are:"))
            {
                int modsIndex = buffer.LastIndexOf(':') + 1;
                var modsArr = buffer.Substring(modsIndex).Split(',');
                _mods = modsArr.Select(x => x.Trim()).ToList();
                _modWait.Set();
            }
        }

        private void ModLockEvent(object sender, ElapsedEventArgs e)
        {
            _modCheckLock = false;
            _modLockTimer.Stop();
        }

        private async Task<List<string>> GetMods()
        {
            if (!_modCheckLock)
            {
                _modCheckLock = true;
                _modLockTimer.Start();
                await WriteLine($"PRIVMSG {ChannelName} /mods");
                await _modWait.WaitAsync();
            }

            return _mods;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _messageHandler?.Dispose();
            _osuUserService?.Dispose();
            _modLockTimer?.Dispose();
        }
    }
}