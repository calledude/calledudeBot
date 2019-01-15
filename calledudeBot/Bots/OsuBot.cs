namespace calledudeBot.Bots
{
    public class OsuBot : IrcClient
    {
        public OsuBot(string token, string osuNick) : base("cho.ppy.sh")
        {
            this.token = token;
            channelName = nick = osuNick;
            instanceName = "osu!";
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