namespace calledudeBot.Bots
{
    public sealed class OsuBot : IrcClient
    {
        public OsuBot(string token, string osuNick) : base("cho.ppy.sh", "osu!")
        {
            Token = token;
            channelName = nick = osuNick;
        }

        public override async void Listen()
        {
            while (true)
            {
                buf = await input.ReadLineAsync();

                if (buf.StartsWith("PING "))
                {
                    string pong = buf.Replace("PING", "PONG");
                    WriteLine(pong);
                    TryLog(pong);
                }
            }
        }
    }
}