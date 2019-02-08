using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public sealed class OsuBot : IrcClient
    {
        public OsuBot(string token, string osuNick) : base("cho.ppy.sh", "osu!", 376)
        {
            Token = token;
            channelName = nick = osuNick;
        }

        protected override async Task Listen()
        {
            while (true)
            {
                buf = await input.ReadLineAsync();

                if (buf.Split(' ')[1] == "QUIT") continue;
                if (buf.StartsWith("PING "))
                    await SendPong();
            }
        }
    }
}