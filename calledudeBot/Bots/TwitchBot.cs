using calledudeBot.Chat;
using calledudeBot.Config;
using calledudeBot.Models;
using calledudeBot.Services;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public sealed class TwitchBot : IrcClient
    {
        private List<string> _mods;
        private DateTime _lastModCheck;
        private readonly AsyncAutoResetEvent _modWait;
        private readonly MessageDispatcher _dispatcher;

        protected override List<string> Failures { get; }
        protected override string Token { get; }

        public TwitchBot(
            BotConfig config,
            MessageDispatcher dispatcher,
            ILogger<TwitchBot> logger)
            : base(logger, "irc.chat.twitch.tv", 366, config.TwitchBotUsername, config.TwitchChannel)
        {
            Failures = new List<string>
            {
                ":tmi.twitch.tv NOTICE * :Improperly formatted auth",
                ":tmi.twitch.tv NOTICE * :Login authentication failed",
            };

            Token = config.TwitchToken;

            _modWait = new AsyncAutoResetEvent();

            Ready += OnReady;
            MessageReceived += HandleMessage;
            UnhandledMessage += HandleRawMessage;
            _dispatcher = dispatcher;
        }

        private async Task OnReady()
        {
            await WriteLine("CAP REQ :twitch.tv/commands");
            await _dispatcher.PublishAsync(new ReadyNotification(this));
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

        private async Task<List<string>> GetMods()
        {
            if (DateTime.Now > _lastModCheck.AddMinutes(1))
            {
                _lastModCheck = DateTime.Now;
                await WriteLine($"PRIVMSG {ChannelName} /mods");
                await _modWait.WaitAsync();
            }

            return _mods;
        }
    }
}