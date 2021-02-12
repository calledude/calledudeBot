using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Config;
using calledudeBot.Models;
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
        private readonly OsuBot _osuBot;
        private readonly APIHandler<OsuSong> _api;
        private readonly TwitchBot _twitchBot;

        public SongRequester(BotConfig config, TwitchBot twitchBot, OsuBot osuBot, APIHandler<OsuSong> api)
        {
            _osuAPIToken = config.OsuAPIToken;
            _osuBot = osuBot;
            _api = api;
            _twitchBot = twitchBot;
        }

        //[http://osu.ppy.sh/b/795232 fhana - Wonder Stella [Stella]]
        public async Task Handle(IrcMessage notification, CancellationToken cancellationToken)
        {
            if (!notification.Content.Contains("://osu.ppy.sh/b/"))
                return;

            var num = notification.Content
                .SkipWhile(x => !char.IsDigit(x))
                .TakeWhile(char.IsDigit);

            var beatmapID = string.Concat(num);
            var reqLink = string.Format(_songRequestLink, _osuAPIToken, beatmapID);

            var song = await _api.RequestOnce(reqLink);
            if (song != null)
            {
                var response = notification.CloneWithMessage($"[http://osu.ppy.sh/b/{beatmapID} {song.Artist} - {song.Title} [{song.BeatmapVersion}]]");
                await _osuBot.SendMessageAsync(response);
            }
            else
            {
                var response = notification.CloneWithMessage("I couldn't find that song, sorry.");
                await _twitchBot.SendMessageAsync(response);
            }
        }
    }
}
