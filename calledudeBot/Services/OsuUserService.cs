using calledudeBot.Bots;
using calledudeBot.Chat;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public class OsuUserService : IDisposable
    {
        private OsuUser oldOsuData;
        private readonly APIHandler<OsuUser> api;
        private readonly TwitchBot _twitch;

        public OsuUserService(string osuAPIToken, string osuNick, TwitchBot twitch)
        {
            api = new APIHandler<OsuUser>($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}");
            api.DataReceived += checkUserUpdateAsync;
            _twitch = twitch;
        }

        public async Task Start()
        {
            await api.Start();
        }

        private async void checkUserUpdateAsync(OsuUser user)
        {
            if (user == null) throw new ArgumentException("Invalid username.", nameof(user));

            if (oldOsuData != null && oldOsuData.Rank != user.Rank && Math.Abs(user.PP - oldOsuData.PP) >= 0.1)
            {
                int rankDiff = user.Rank - oldOsuData.Rank;
                float ppDiff = user.PP - oldOsuData.PP;

                string formatted = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", Math.Abs(ppDiff));
                string totalPP = user.PP.ToString(CultureInfo.InvariantCulture);

                string rankMessage = $"{Math.Abs(rankDiff)} ranks (#{user.Rank}). ";
                string ppMessage = $"PP: {(ppDiff < 0 ? "-" : "+")}{formatted}pp ({totalPP}pp)";

                var newRankMessage = new IrcMessage($"{user.Username} just {(rankDiff < 0 ? "gained" : "lost")} {rankMessage} {ppMessage}");
                await _twitch.SendMessageAsync(newRankMessage);
            }
            oldOsuData = user;
        }

        public void Dispose()
        {
            api.Dispose();
        }
    }
}
