namespace calledudeBot.Chat
{
    public sealed class DiscordMessage : Message
    {
        public ulong Destination { get; }

        public DiscordMessage(string message, User sender, ulong destination) 
            : base(message, sender)
        {
            Destination = destination;
        }
    }
}
