using Newtonsoft.Json;

namespace calledudeBot.Config
{
    public class BotConfig
    {
        internal BotConfig()
        {
        }

        [JsonConstructor]
        public BotConfig(
            string discordToken,
            ulong streamerId,
            ulong announceChannelId,
            string osuIRCToken,
            string osuAPIToken,
            string osuUsername,
            string twitchToken,
            string twitchChannel,
            string twitchBotUsername)
        {
            DiscordToken = discordToken;
            StreamerId = streamerId;
            AnnounceChannelId = announceChannelId;
            OsuIRCToken = osuIRCToken;
            OsuAPIToken = osuAPIToken;
            OsuUsername = osuUsername;
            TwitchToken = twitchToken;
            TwitchChannel = twitchChannel;
            TwitchBotUsername = twitchBotUsername;
        }

        public string DiscordToken { get; }
        public ulong StreamerId { get; }
        public ulong AnnounceChannelId { get; }

        public string OsuIRCToken { get; }
        public string OsuAPIToken { get; }
        public string OsuUsername { get; }

        public string TwitchToken { get; }
        public string TwitchChannel { get; }
        public string TwitchBotUsername { get; }
    }
}
