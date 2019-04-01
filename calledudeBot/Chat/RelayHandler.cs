using calledudeBot.Bots;
using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Chat
{
    public sealed class RelayHandler : MessageHandler<IrcMessage>, IDisposable
    {
        private readonly Queue<IrcMessage> _messageQueue;
        private DateTime _lastMessage;
        private readonly Timer _relayTimer;
        private readonly string _streamerNick;
        private readonly Bot<IrcMessage> _relaySubject;
        private readonly SongRequester _songRequester;
        private readonly TwitchBot _twitch;

        public RelayHandler(TwitchBot twitch, string streamerNick, string osuAPIToken, Bot<IrcMessage> relaySubject) : base(twitch)
        {
            _lastMessage = DateTime.Now;
            _messageQueue = new Queue<IrcMessage>();
            _songRequester = new SongRequester(osuAPIToken);

            _twitch = twitch;
            _relaySubject = relaySubject;

            _streamerNick = streamerNick.Substring(1).ToLower();
            _relayTimer = new Timer(200);
            _relayTimer.Elapsed += TryRelay;
            _relayTimer.Start();
        }

        new public async Task DetermineResponse(IrcMessage message)
        {
            if (!await base.DetermineResponse(message))
            {
                if (message.Content.Contains("://osu.ppy.sh/b/"))
                {
                    await _songRequester.RequestSong(message);
                }
                if (message.Sender.Name.ToLower() != _streamerNick) //Only relay messages that aren't from the streamer
                {
                    _messageQueue.Enqueue(message);
                    TryRelay(null, null);
                }
            }
        }

        private async void TryRelay(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now - _lastMessage > TimeSpan.FromMilliseconds(500) && _messageQueue.Count > 0)
            {
                await Relay(_messageQueue.Dequeue());
                _lastMessage = DateTime.Now;
            }
        }

        private async Task Relay(IrcMessage message)
        {
            message.Content = $"{message.Sender.Name}: {message.Content}";
            _twitch.TryLog($"-> {_relaySubject.Name}: {message.Content}");
            await _relaySubject.SendMessageAsync(message);
        }

        public void Dispose()
        {
            _relayTimer.Dispose();
        }
    }
}
