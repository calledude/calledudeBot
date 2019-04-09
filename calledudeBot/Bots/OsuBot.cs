using System.Collections.Generic;

namespace calledudeBot.Bots
{
    public sealed class OsuBot : IrcClient
    {
        protected override List<string> Failures { get; }

        public OsuBot(string token, string osuNick) 
            : base("cho.ppy.sh", token, "osu!", 376, osuNick)
        {
            Failures = new List<string>
            {
                $":cho.ppy.sh 464 {Nick} :Bad authentication token.",
            };
        }
    }
}