namespace calledudeBot.Chat
{
    public sealed class DiscordMessage : Message<DiscordMessage>
    {
        public ulong Destination { get; }

        public DiscordMessage(
            string message,
            string channel,
            User sender,
            ulong destination) : base(message, channel, sender)
        {
            Destination = destination;
        }

        public override DiscordMessage CloneWithMessage(string message)
            => new DiscordMessage(message, Channel, Sender, Destination);
    }
}
