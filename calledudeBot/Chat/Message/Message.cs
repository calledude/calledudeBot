namespace calledudeBot.Chat
{
    public abstract class Message
    {
        public User Sender { get; protected set; }
        public string Content { get; set; }

        protected Message(string message)
        {
            Content = message;
        }
    }
}