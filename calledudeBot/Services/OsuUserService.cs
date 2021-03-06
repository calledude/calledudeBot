﻿using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Config;
using calledudeBot.Models;
using MediatR;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace calledudeBot.Services
{
    public sealed class OsuUserService : INotificationHandler<ReadyNotification>, IDisposable
    {
        private OsuUser _oldOsuData;
        private readonly string _osuAPIToken;
        private readonly string _osuNick;
        private readonly APIHandler<OsuUser> _api;
        private readonly TwitchBot _twitch;

        public OsuUserService(APIHandler<OsuUser> api, BotConfig config, TwitchBot twitchBot)
        {
            _osuAPIToken = config.OsuAPIToken;
            _osuNick = config.OsuUsername;

            _api = api;
            _twitch = twitchBot;
            _api.DataReceived += CheckUserUpdateAsync;
        }

        public async Task Handle(ReadyNotification notification, CancellationToken cancellationToken)
        {
            if (!(notification.Bot is TwitchBot))
                return;

            await _api.Start(string.Format("https://osu.ppy.sh/api/get_user?k={0}&u={1}", _osuAPIToken, _osuNick));
        }

        private async void CheckUserUpdateAsync(OsuUser user)
        {
            if (user == null)
                throw new ArgumentNullException("Invalid username.", nameof(user));

            if (_oldOsuData != null
                && _oldOsuData.Rank != user.Rank
                && Math.Abs(user.PP - _oldOsuData.PP) >= 0.1)
            {
                var rankDiff = user.Rank - _oldOsuData.Rank;
                var ppDiff = user.PP - _oldOsuData.PP;

                var formatted = string.Format(CultureInfo.InvariantCulture, "{0:0.00}", Math.Abs(ppDiff));
                var totalPP = user.PP.ToString(CultureInfo.InvariantCulture);

                var rankMessage = $"{Math.Abs(rankDiff)} ranks (#{user.Rank}). ";
                var ppMessage = $"PP: {(ppDiff < 0 ? "-" : "+")}{formatted}pp ({totalPP}pp)";

                var newRankMessage = new IrcMessage($"{user.Username} just {(rankDiff < 0 ? "gained" : "lost")} {rankMessage} {ppMessage}");

                await _twitch.SendMessageAsync(newRankMessage);
            }

            _oldOsuData = user;
        }

        public void Dispose()
            => _api.Dispose();
    }
}
