using calledudeBot.Bots;
using Discord.WebSocket;
using System.Linq;

namespace calledudeBot.Chat
{
    public sealed class DiscordUser : User
    {
        private static readonly DiscordBot _discord = calledudeBot.discordBot;
        private readonly SocketUser user;

        public DiscordUser(SocketUser user) : base(user.Username)
        {
            this.user = user;
        }

        internal override bool IsMod
        {
            get
            {
                var mods = _discord.GetModerators();
                return mods.Any(u => u.Id == user.Id) || _discord.IsMod(user as SocketGuildUser);
            }
        }
    }
}
