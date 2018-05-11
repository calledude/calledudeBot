using calledudeBot.Bots;
using Discord.WebSocket;
using System.Collections.Generic;

namespace calledudeBot.Chat
{
    public class User
    {
        private static TwitchBot twitch = calledudeBot.twitchBot;
        private static DiscordBot discord = calledudeBot.discordBot;
        private SocketUser user;
        public string Name { get; }
        public bool isMod
        {
            get { return isAllowed(Name); }
        }

        public User(string name)
        {
            Name = name;
        }
        public User(SocketUser user)
        {
            this.user = user;
            Name = user.Username;
        }

        private bool isAllowed(string user)
        {
            List<string> mods = twitch.getMods();
            foreach (string m in mods)
            {
                if (m == user.ToLower()) return true;
            }
            SocketRole adminRole = discord.getAdminRole();
            foreach (SocketGuildUser usr in adminRole.Members)
            {
                if (usr.Id == this.user?.Id) return true;
            }
            return false;
        }
    }
}
