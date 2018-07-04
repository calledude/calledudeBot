using calledudeBot.Bots;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading;

namespace calledudeBot.Chat
{
    public class User
    {
        private static TwitchBot twitch = calledudeBot.twitchBot;
        private static DiscordBot discord = calledudeBot.discordBot;
        private AutoResetEvent ev = new AutoResetEvent(false);
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
            if(this.user == null)
            {
                twitch.requestMods(ev);
                ev.WaitOne(250);

                List<string> mods = twitch.getMods();
                foreach (string m in mods)
                {
                    if (string.Compare(m, user, true) == 0) return true;
                }
            }
            else
            {
                SocketRole adminRole = discord.getAdminRole();
                foreach (SocketGuildUser usr in adminRole.Members)
                {
                    if (usr.Id == this.user?.Id) return true;
                }
            }
            return false;
        }
    }
}
