using calledudeBot.Bots;
using System;
using System.Linq;

namespace calledudeBot.Chat
{
    public class IrcUser : User
    {
        private static readonly TwitchBot twitch = calledudeBot.twitchBot;

        public IrcUser(string name) : base(name)
        {
        }

        internal override bool IsMod
        {
            get
            {
                var mods = twitch.GetMods();
                return mods.Any(u => u.Equals(Name, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
