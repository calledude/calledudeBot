using System.Linq;
using calledudeBot.Bots;

namespace calledudeBot.Chat
{
    public class Message
    {
        public string Content { get; set; }
        public User Sender{ get; set; }
        public Bot Origin { get; }
        public ulong Destination { get; set;}

        public Message(string message)
        {
            Content = message;
        }

        public Message(string message, Bot bot)
        {
            Origin = bot;
            Content = message;
            if(typeof(TwitchBot) == bot.GetType())
            {
                decodeMessage();
            }
        }

        //:calledude!calledude@calledude.tmi.twitch.tv PRIVMSG #calledude :hej
        private void decodeMessage()
        {
            //Get name of sender
            var indexUpper = Content.IndexOf('!');
            var indexLower = Content.IndexOf(':') + 1;
            var nameLength = indexUpper - indexLower;

            var name = Content.Substring(indexLower, nameLength);
            Sender = new User(char.ToUpper(name.First()) + name.Substring(1).ToLower()); //capitalize first letter in username

            //Get content
            var stringFormatted = new string[Content.Split(' ').Length - 3];
            var counter = 0;

            for (var i = 3; i < Content.Split(' ').Length; i++)
            {
                stringFormatted[counter] = Content.Split(' ')[i];
                counter++;
            }
            Content = string.Join(" ", stringFormatted).Substring(1); //Deletes ":" from the first letter in the message
        }
    }
}