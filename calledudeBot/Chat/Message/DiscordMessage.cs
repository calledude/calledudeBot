namespace calledudeBot.Chat
{
    public class DiscordMessage : Message
    {
        public ulong Destination { get; set; }

        public DiscordMessage(string message) : base(message)
        {
        }
    }
}
