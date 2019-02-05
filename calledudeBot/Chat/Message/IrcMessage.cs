using System.Linq;

namespace calledudeBot.Chat
{
    public class IrcMessage : Message
    {
        public IrcMessage(string message) : base(message)
        {
            parseMessage();
        }

        //:calledude!calledude@calledude.tmi.twitch.tv PRIVMSG #calledude :hej
        private void parseMessage()
        {
            //Get name of sender
            var indexUpper = Content.IndexOf('!');
            var nameLength = indexUpper - 1;

            var name = Content.Substring(1, nameLength);
            Sender = new IrcUser(char.ToUpper(name[0]) + name.Substring(1)); //capitalize first letter in username

            //Get content
            Content = string.Join(" ", Content.Split(' ').Skip(3)).Substring(1);
        }
    }
}
