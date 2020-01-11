using calledudeBot.Config;
using System.Collections.Generic;

namespace calledudeBot.Bots
{
    public sealed class OsuBot : IrcClient
    {
        protected override List<string> Failures { get; }

        protected override string Token { get; }

        public OsuBot(BotConfig config)
            : base("cho.ppy.sh", 376, config.OsuUsername)
        {
            Name = "osu!";
            Token = config.OsuIRCToken;
            Failures = new List<string>
            {
                $":cho.ppy.sh 464 {Nick} :Bad authentication token.",
            };
        }
    }
}