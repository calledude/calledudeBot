using calledudeBot.Bots;
using calledudeBot.Chat.Commands;
using calledudeBot.Config;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public sealed class RelayHandler : IDisposable, INotificationHandler<IrcMessage>
    {
        private readonly Queue<IrcMessage> _messageQueue;
        private DateTime _lastMessage;
        private readonly Timer _relayTimer;
        private readonly string _streamerNick;
        private readonly ILogger<RelayHandler> _logger;
        private readonly OsuBot _relaySubject;
        private readonly TwitchBot _twitch;

        public RelayHandler(
            ILogger<RelayHandler> logger,
            OsuBot osuBot,
            BotConfig config,
            TwitchBot twitch)
        {
            _lastMessage = DateTime.Now;
            _messageQueue = new Queue<IrcMessage>();
            _logger = logger;
            _relaySubject = osuBot;

            _streamerNick = config.TwitchChannel.Substring(1).ToLower();
            _relayTimer = new Timer(TryRelay, null, 0, 200);
            _twitch = twitch;
        }

        public Task Handle(IrcMessage notification, CancellationToken cancellationToken)
        {
            if (CommandUtils.IsCommand(notification.Content))
            {
                return Task.CompletedTask;
            }

            //Only relay messages that aren't from the streamer
            if (!_streamerNick.Equals(notification.Sender.Name, StringComparison.OrdinalIgnoreCase))
            {
                _messageQueue.Enqueue(notification);
                TryRelay(null);
            }

            return Task.CompletedTask;
        }

        private async void TryRelay(object state)
        {
            if (DateTime.Now - _lastMessage > TimeSpan.FromMilliseconds(500) && _messageQueue.Count > 0)
            {
                await Relay(_messageQueue.Dequeue());
                _lastMessage = DateTime.Now;
            }
        }

        private async Task Relay(IrcMessage message)
        {
            var response = message.CloneWithMessage($"{message.Sender.Name}: {message.Content}");
            _logger.LogInformation("Twitch -> osu!: {0}", response.Content);
            await _relaySubject.SendMessageAsync(response);
        }

        public void Dispose()
        {
            _relayTimer.Dispose();
        }
    }
}
