using calledudeBot.Chat;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public class SongRequester
    {
        private const string _songRequestLink = "https://osu.ppy.sh/api/get_beatmaps?k={0}&b={1}";
        private readonly string _osuAPIToken;

        public SongRequester(string osuAPIToken)
        {
            _osuAPIToken = osuAPIToken;
        }

        //[http://osu.ppy.sh/b/795232 fhana - Wonder Stella [Stella]]
        public async Task RequestSong(Message message)
        {
            var idx = message.Content.IndexOf("/b/") + "/b/".Length;
            var num = message.Content.Skip(idx).TakeWhile(c => char.IsNumber(c));
            var beatmapID = string.Concat(num);
            var reqLink = string.Format(_songRequestLink, _osuAPIToken, beatmapID);

            using (var api = new APIHandler<OsuSong>(reqLink))
            {
                OsuSong song = await api.RequestOnce();
                if (song != null)
                {
                    message.Content = $"[http://osu.ppy.sh/b/{beatmapID} {song.Artist} - {song.Title} [{song.BeatmapVersion}]]";
                }
                else
                {
                    message.Content = "I couldn't find that song, sorry.";
                }
            }
        }

    }
}
