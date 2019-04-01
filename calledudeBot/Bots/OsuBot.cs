namespace calledudeBot.Bots
{
    public sealed class OsuBot : IrcClient
    {
        public OsuBot(string token, string osuNick) : base("cho.ppy.sh", "osu!", 376)
        {
            Token = token;
            Nick = osuNick;
        }
    }
}