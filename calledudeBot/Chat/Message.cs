using System.Linq;
using calledudeBot.Bots;

namespace calledudeBot.Chat
{
    public class Message
    {
        public string Content { get; set; }
        public User Sender{ get; set; }
        public Bot Origin { get; }
        public ulong Destination { get; set; }

        public Message(string message, Bot bot = null)
        {
            Origin = bot;
            Content = message;
            if(bot is IrcClient)
            {
                parseMessage(this);
            }
        }

        //:calledude!calledude@calledude.tmi.twitch.tv PRIVMSG #calledude :hej
        private static void parseMessage(Message message)
        {
            //Get name of sender
            var indexUpper = message.Content.IndexOf('!');
            var nameLength = indexUpper - 1;

            var name = message.Content.Substring(1, nameLength);
            message.Sender = new User(char.ToUpper(name.First()) + name.Substring(1)); //capitalize first letter in username

            //Get content
            message.Content = string.Join(" ", message.Content.Split(' ').Skip(3)).Substring(1);
        }
    }
}