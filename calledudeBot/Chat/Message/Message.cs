using MediatR;

namespace calledudeBot.Chat
{
    public abstract class Message : INotification
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
    }

    public abstract class Message<T> : Message
    {
        protected Message(string message)
            : base(message)
        {
        }

        protected Message(string message, string channel, User sender)
            : base(message, channel, sender)
        {
        }

        public abstract T CloneWithMessage(string message);
    }
}