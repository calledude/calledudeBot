namespace calledudeBot.Chat
{
    public sealed class User
    {
        internal string Name { get; }
        internal bool IsMod { get; }

        public User(string userName, bool isMod)
        {
            Name = userName;
            IsMod = isMod;
        }
    }
}
