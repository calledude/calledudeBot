namespace calledudeBot.Bots
{
    public class OsuBot : IrcClient
    {
        public OsuBot(string token, string osuNick) : base("cho.ppy.sh", "osu!")
        {
            Token = token;
            channelName = nick = osuNick;
        }

        public override void Listen()
        {
            for (buf = input.ReadLine(); ; buf = input.ReadLine())
            {
                if (buf.StartsWith("PING "))
                {
                    string pong = buf.Replace("PING", "PONG");
                    WriteLine(pong);
                    tryLog(pong);
                }
            }
        }
    }
}