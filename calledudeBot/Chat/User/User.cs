namespace calledudeBot.Chat
{
    public abstract class User
    {
        internal readonly string Name;
        internal abstract bool IsMod { get; }

        protected User(string userName)
        {
            Name = userName;
        }
    }
}
