using calledudeBot.Bots;
using System.Collections.Generic;

namespace calledudeBot.Chat
{
    public class User
    {
        private static TwitchBot twitch = Common.calledudeBot.twitchBot;
        public string Name { get; }
        public bool isMod
        {
            get { return isAllowed(Name); }
        }

        public User(string name)
        {
            Name = name;
        }
        
        private bool isAllowed(string user)
        {
            List<string> mods = twitch.getMods();
            foreach (string m in mods)
            {
                if (m == user.ToLower()) return true;
            }
            return false;
        }
    }
}
