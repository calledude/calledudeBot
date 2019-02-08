using calledudeBot.Bots;
using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace calledudeBot.Chat
{
    public sealed class RelayHandler : MessageHandler<IrcMessage>, IDisposable
    {
        private readonly OsuBot osu;
        private readonly Queue<IrcMessage> messageQueue = new Queue<IrcMessage>();
        private DateTime lastMessage;
        private readonly Timer relayTimer;
        private readonly string osuAPIToken, streamerNick;
        private const string songRequestLink = "https://osu.ppy.sh/api/get_beatmaps?k={0}&b={1}";

        public RelayHandler(IrcClient bot, string streamerNick, string osuAPIToken) : base(bot)
        {
            osu = calledudeBot.osuBot;
            this.osuAPIToken = osuAPIToken;
            this.streamerNick = streamerNick.Substring(1).ToLower();
            relayTimer = new Timer(200);
            relayTimer.Elapsed += tryRelay;
            relayTimer.Start();
        }

        new public void DetermineResponse(IrcMessage message)
        {
            if (!base.DetermineResponse(message))
            {
                if (message.Content.Contains("://osu.ppy.sh/b/"))
                {
                    requestSong(message);
                }
                if (message.Sender.Name.ToLower() != streamerNick) //We only want to relay messages from twitch
                {
                    messageQueue.Enqueue(message);
                    tryRelay(null, null);
                }
            }
        }

        private void tryRelay(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now - lastMessage > TimeSpan.FromMilliseconds(500) && messageQueue.Count > 0)
            {
                relay(messageQueue.Dequeue());
                lastMessage = DateTime.Now;
            }
        }

        private void relay(IrcMessage message)
        {
            message.Content = $"{message.Sender.Name}: {message.Content}";
            osu.SendMessage(message);
        }

        //[http://osu.ppy.sh/b/795232 fhana - Wonder Stella [Stella]]
        private async void requestSong(Message message)
        {
            var idx = message.Content.IndexOf("/b/") + "/b/".Length;
            var num = message.Content.Skip(idx).TakeWhile(c => char.IsNumber(c));
            var beatmapID = string.Concat(num);
            var reqLink = string.Format(songRequestLink, osuAPIToken, beatmapID);

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
