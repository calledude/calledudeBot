namespace calledudeBot.Chat
{
    public abstract class Message
    {
        public string Content { get; set; }
        public User Sender { get; }

        protected Message(string message)
        {
            Content = message;
        }

        protected Message(string message, User sender) : this(message)
        {
            Sender = sender;
        }

    }
}