using calledudeBot.Bots;
using calledudeBot.Config;
using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using System.Threading;
using calledudeBot.Chat.Commands;

namespace calledudeBot.Chat
{
    public sealed class RelayHandler : IDisposable, INotificationHandler<IrcMessage>
    {
        private readonly Queue<IrcMessage> _messageQueue;
        private DateTime _lastMessage;
        private readonly Timer _relayTimer;
        private readonly string _streamerNick;
        private readonly OsuBot _relaySubject;
        private readonly SongRequester _songRequester;
        private readonly TwitchBot _twitch;

        public RelayHandler(
            OsuBot osuBot,
            BotConfig config,
            SongRequester songRequester,
            TwitchBot twitch)
        {
            _lastMessage = DateTime.Now;
            _messageQueue = new Queue<IrcMessage>();
            _songRequester = songRequester;

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
            message.Response = $"{message.Sender.Name}: {message.Content}";
            _twitch.Log($"-> {_relaySubject.Name}: {message.Response}");
            await _relaySubject.SendMessageAsync(message);
        }

        public void Dispose()
        {
            _relayTimer.Dispose();
        }
    }
}
