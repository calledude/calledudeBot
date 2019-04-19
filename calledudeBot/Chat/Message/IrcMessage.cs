using System.Linq;

namespace calledudeBot.Chat
{
    public sealed class IrcMessage : Message
    {
        public IrcMessage(string message) : base(message)
        {
        }

        public IrcMessage(string message, string channel, User sender)
            : base(message, channel, sender)
        {
        }

        public static string ParseMessage(string buffer)
        {
            return string.Join(" ", buffer.Split(' ').Skip(3)).Substring(1);
        }

        public static string ParseUser(string buffer)
        {
            //Get name of sender
            var indexUpper = buffer.IndexOf('!');
            var nameLength = indexUpper - 1;

            var name = buffer.Substring(1, nameLength);

            return char.ToUpper(name[0]) + name.Substring(1); //capitalize first letter in username
        }
    }
}
