using calledudeBot.Bots;
using System.Linq;
using Discord.WebSocket;

namespace calledudeBot.Chat
{
    public class DiscordUser : User
    {
        private static readonly DiscordBot discord = calledudeBot.discordBot;
        private readonly SocketUser user;

        public DiscordUser(SocketUser user) : base(user.Username)
        {
            this.user = user;
        }

        internal override bool IsMod
        {
            get
            {
                var mods = discord.GetModerators();
                return mods.Any(u => u.Id == user.Id) || discord.IsMod(user as SocketGuildUser);
            }
        }
    }
}
