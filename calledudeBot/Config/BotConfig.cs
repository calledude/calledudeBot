namespace calledudeBot.Config
{
    public class BotConfig
    {
        public string DiscordToken { get; set; }
        public ulong StreamerId { get; set; }
        public ulong AnnounceChannelId { get; set; }

        public string OsuIRCToken { get; set; }
        public string OsuAPIToken { get; set; }
        public string OsuUsername { get; set; }

        public string TwitchToken { get; set; }
        public string TwitchChannel { get; set; }
        public string TwitchBotUsername { get; set; }
    }
}
