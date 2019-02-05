namespace calledudeBot.Chat
{
    public abstract class Message
    {
        public string Content { get; set; }
        public User Sender { get; set; }

        protected Message(string message)
        {
            Content = message;
        }
    }
}