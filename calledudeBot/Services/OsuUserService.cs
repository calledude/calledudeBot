using calledudeBot.Bots;
using calledudeBot.Chat;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public sealed class OsuUserService : IDisposable
    {
        private OsuUser _oldOsuData;
        private readonly APIHandler<OsuUser> _api;
        private readonly TwitchBot _twitch;

        public OsuUserService(string osuAPIToken, string osuNick, TwitchBot twitch)
        {
            _api = new APIHandler<OsuUser>($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}");
            _api.DataReceived += CheckUserUpdateAsync;
            _twitch = twitch;
        }

        public async Task Start()
        {
            await _api.Start();
        }

        private async void CheckUserUpdateAsync(OsuUser user)
        {
            if (user == null) throw new ArgumentNullException("Invalid username.", nameof(user));

            if (_oldOsuData != null 
                && _oldOsuData.Rank != user.Rank 
                && Math.Abs(user.PP - _oldOsuData.PP) >= 0.1)
            {
                int rankDiff = user.Rank - _oldOsuData.Rank;
                float ppDiff = user.PP - _oldOsuData.PP;

                string formatted = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", Math.Abs(ppDiff));
                string totalPP = user.PP.ToString(CultureInfo.InvariantCulture);

                string rankMessage = $"{Math.Abs(rankDiff)} ranks (#{user.Rank}). ";
                string ppMessage = $"PP: {(ppDiff < 0 ? "-" : "+")}{formatted}pp ({totalPP}pp)";

                var newRankMessage = new IrcMessage($"{user.Username} just {(rankDiff < 0 ? "gained" : "lost")} {rankMessage} {ppMessage}");
                await _twitch.SendMessageAsync(newRankMessage);
            }
            _oldOsuData = user;
        }

        public void Dispose()
        {
            _api.Dispose();
        }
    }
}
