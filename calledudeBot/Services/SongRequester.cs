using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Config;
using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public sealed class SongRequester : INotificationHandler<IrcMessage>
    {
        private const string _songRequestLink = "https://osu.ppy.sh/api/get_beatmaps?k={0}&b={1}";
        private readonly string _osuAPIToken;
        private readonly TwitchBot _bot;

        public SongRequester(BotConfig config, TwitchBot bot)
        {
            _osuAPIToken = config.OsuAPIToken;
            _bot = bot;
        }

        //[http://osu.ppy.sh/b/795232 fhana - Wonder Stella [Stella]]
        public async Task Handle(IrcMessage notification, CancellationToken cancellationToken)
        {
            if (!notification.Content.Contains("://osu.ppy.sh/b/"))
            {
                return;
            }

            var idx = notification.Content.IndexOf("/b/") + "/b/".Length;
            var num = notification.Content.Skip(idx).TakeWhile(char.IsDigit);
            var beatmapID = string.Concat(num);
            var reqLink = string.Format(_songRequestLink, _osuAPIToken, beatmapID);

            using (var api = new APIHandler<OsuSong>(reqLink))
            {
                OsuSong song = await api.RequestOnce();
                if (song != null)
                {
                    notification.Response = $"[http://osu.ppy.sh/b/{beatmapID} {song.Artist} - {song.Title} [{song.BeatmapVersion}]]";
                }
                else
                {
                    notification.Response = "I couldn't find that song, sorry.";
                }
                await _bot.SendMessageAsync(notification);
            }
        }
    }
}
