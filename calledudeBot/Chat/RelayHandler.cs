using calledudeBot.Bots;
using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Chat
{
    public sealed class RelayHandler : MessageHandler<IrcMessage>, IDisposable
    {
        private readonly Queue<IrcMessage> messageQueue = new Queue<IrcMessage>();
        private DateTime lastMessage = DateTime.Now;
        private readonly Timer relayTimer;
        private readonly string osuAPIToken, streamerNick;
        private const string _songRequestLink = "https://osu.ppy.sh/api/get_beatmaps?k={0}&b={1}";
        private readonly Bot<IrcMessage> _relaySubject;
        private readonly TwitchBot _twitch;

        public RelayHandler(TwitchBot twitch, string streamerNick, string osuAPIToken, Bot<IrcMessage> relaySubject) : base(twitch)
        {
            _twitch = twitch;
            _relaySubject = relaySubject;
            this.osuAPIToken = osuAPIToken;
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
                    requestSong(message);
                }
                if (message.Sender.Name != streamerNick) //Only relay messages that aren't from the streamer
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

        //[http://osu.ppy.sh/b/795232 fhana - Wonder Stella [Stella]]
        private async void requestSong(Message message)
        {
            var idx = message.Content.IndexOf("/b/") + "/b/".Length;
            var num = message.Content.Skip(idx).TakeWhile(c => char.IsNumber(c));
            var beatmapID = string.Concat(num);
            var reqLink = string.Format(_songRequestLink, osuAPIToken, beatmapID);

            using (var api = new APIHandler<OsuSong>(reqLink))
            {
                OsuSong song = await api.RequestOnce();
                if (song != null)
                {
                    message.Content = $"[http://osu.ppy.sh/b/{beatmapID} {song.Artist} - {song.Title} [{song.BeatmapVersion}]]";
                }
            }
        }

        public void Dispose()
        {
            relayTimer.Dispose();
        }
    }
}
