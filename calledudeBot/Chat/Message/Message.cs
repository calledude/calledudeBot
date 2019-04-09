namespace calledudeBot.Chat
{
    public abstract class Message
    {
        public string Content { get; set; }
        public User Sender { get; }
        public string Channel { get; }

        protected Message(string message)
        {
            Content = message;
        }

        protected Message(string message, string channel, User sender) : this(message)
        {
            Sender = sender;
            Channel = channel;
        }
    }
}