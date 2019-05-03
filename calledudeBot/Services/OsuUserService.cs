using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Config;
using calledudeBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public sealed class OsuUserService : IDisposable
    {
        private OsuUser _oldOsuData;
        private readonly APIHandler<OsuUser> _api;
        private readonly IServiceProvider _serviceProvider;

        public OsuUserService(BotConfig config, IServiceProvider serviceProvider)
        {
            var osuAPIToken = config.OsuAPIToken;
            var osuNick = config.OsuUsername;

            _api = new APIHandler<OsuUser>($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}");
            _api.DataReceived += CheckUserUpdateAsync;
            _serviceProvider = serviceProvider;
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

                await _serviceProvider
                    .GetRequiredService<TwitchBot>()
                    .SendMessageAsync(newRankMessage);
            }
            _oldOsuData = user;
        }

        public void Dispose()
        {
            _api.Dispose();
        }
    }
}
