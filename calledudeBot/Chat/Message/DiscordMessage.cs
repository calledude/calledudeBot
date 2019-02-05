namespace calledudeBot.Chat
{
    public sealed class DiscordMessage : Message
    {
        public ulong Destination { get; set; }

        public DiscordMessage(string message) : base(message)
        {
        }
    }
}
