using calledudeBot.Chat;
using calledudeBot.Config;
using calledudeBot.Services;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Bots
{
    public sealed class TwitchBot : IrcClient
    {
        private List<string> _mods;
        private readonly OsuUserService _osuUserService;
        private Timer _modLockTimer;
        private bool _modCheckLock;
        private readonly AsyncAutoResetEvent _modWait;
        private readonly MessageDispatcher _dispatcher;

        protected override List<string> Failures { get; }
        protected override string Token { get; }

        public TwitchBot(
            BotConfig config,
            OsuUserService osuUserService,
            MessageDispatcher dispatcher)
            : base("irc.chat.twitch.tv", "Twitch", 366, config.TwitchBotUsername, config.TwitchChannel)
        {
            Failures = new List<string>
            {
                ":tmi.twitch.tv NOTICE * :Improperly formatted auth",
                ":tmi.twitch.tv NOTICE * :Login authentication failed",
            };

            Token = config.TwitchToken;

            _modWait = new AsyncAutoResetEvent();
            _osuUserService = osuUserService;

            Ready += OnReady;
            MessageReceived += HandleMessage;
            UnhandledMessage += HandleRawMessage;
            _dispatcher = dispatcher;
        }

        private async Task OnReady()
        {
            _modLockTimer = new Timer(60000);

            _modLockTimer.Elapsed += ModLockEvent;
            _modLockTimer.Start();

            await WriteLine("CAP REQ :twitch.tv/commands");
            await _osuUserService.Start();
        }

        private async void HandleMessage(string message, string user)
        {
            var sender = new User(user, () => IsMod(user));
            var msg = new IrcMessage(message, ChannelName, sender);

            await _dispatcher.PublishAsync(msg);
        }

        private async Task<bool> IsMod(string user)
        {
            var mods = await GetMods();
            return mods.Any(u => u.Equals(user, StringComparison.OrdinalIgnoreCase));
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

        public override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _osuUserService?.Dispose();
            _modLockTimer?.Dispose();
        }
    }
}