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
        private readonly Queue<IrcMessage> messageQueue = new Queue<IrcMessage>();
        private DateTime lastMessage = DateTime.Now;
        private readonly Timer relayTimer;
        private readonly string streamerNick;
        private readonly Bot<IrcMessage> _relaySubject;
        private readonly SongRequester _songRequester;
        private readonly TwitchBot _twitch;

        public RelayHandler(TwitchBot twitch, string streamerNick, string osuAPIToken, Bot<IrcMessage> relaySubject) : base(twitch)
        {
            _songRequester = new SongRequester(osuAPIToken);

            _twitch = twitch;
            _relaySubject = relaySubject;

            this.streamerNick = streamerNick.Substring(1).ToLower();
            relayTimer = new Timer(200);
            relayTimer.Elapsed += tryRelay;
            relayTimer.Start();
        }

        new public async Task DetermineResponse(IrcMessage message)
        {
            if (!await base.DetermineResponse(message))
            {
                if (message.Content.Contains("://osu.ppy.sh/b/"))
                {
                    await _songRequester.RequestSong(message);
                }
                if (message.Sender.Name.ToLower() != streamerNick) //Only relay messages that aren't from the streamer
                {
                    messageQueue.Enqueue(message);
                    tryRelay(null, null);
                }
            }
        }

        private async void tryRelay(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now - lastMessage > TimeSpan.FromMilliseconds(500) && messageQueue.Count > 0)
            {
                await relay(messageQueue.Dequeue());
                lastMessage = DateTime.Now;
            }
        }

        private async Task relay(IrcMessage message)
        {
            message.Content = $"{message.Sender.Name}: {message.Content}";
            _twitch.TryLog($"-> {_relaySubject.Name}: {message.Content}");
            await _relaySubject.SendMessageAsync(message);
        }

        public void Dispose()
        {
            relayTimer.Dispose();
        }
    }
}
