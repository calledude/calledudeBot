namespace calledudeBot.Chat
{
    public sealed class DiscordMessage : Message
    {
        public ulong Destination { get; }

        public DiscordMessage(string message, string channel, User sender, ulong destination) 
            : base(message, channel, sender)
        {
            Destination = destination;
        }
    }
}
