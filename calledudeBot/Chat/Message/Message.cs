using MediatR;

namespace calledudeBot.Chat
{
    public abstract class Message<T> : INotification
    {
        public string Content { get; }
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

        public abstract T CloneWithMessage(string message);
    }
}