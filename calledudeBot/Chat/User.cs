using calledudeBot.Bots;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;

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
            get { return isAllowed(this); }
        }

        public User(string name)
        {
            Name = name;
        }

        public User(SocketUser user) : this(user.Username)
        {
            this.user = user;
        }

        private static bool isAllowed(User chatter)
        {
            if(chatter.user == null) //Determine if the user is a discord user or not
            {
                List<string> mods = twitch.getMods();
                return mods.Any(u => u.Equals(chatter.Name, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                var mods = discord.getModerators();
                return mods.Any(u => u.Id == chatter.user.Id);
            }
        }
    }
}
