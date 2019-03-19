using calledudeBot.Bots;
using System;
using System.Linq;

namespace calledudeBot.Chat
{
    public sealed class IrcUser : User
    {
        private static readonly TwitchBot _twitch = calledudeBot.twitchBot;

        public IrcUser(string name) : base(name)
        {
        }

        internal override bool IsMod
        {
            get
            {
                var mods = _twitch.GetMods();
                return mods.Any(u => u.Equals(Name, StringComparison.OrdinalIgnoreCase));
            }
        }
    }
}
