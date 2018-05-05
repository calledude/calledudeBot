using System.Linq;
using calledudeBot.Bots;

namespace calledudeBot.Chat
{
    public class Message
    {
        private string message;
        public string Content { get; set; }
        public User Sender{ get; set; }
        public Bot Origin { get; }
        public ulong Destination { get; set;}

        public Message(string message)
        {
            this.message = message;
        }

        public Message(string message, Bot bot)
        {
            Origin = bot;
            this.message = message;
            if(typeof(TwitchBot) == bot.GetType())
            {
                decodeMessage();
            }
        }

        //:calledude!calledude@calledude.tmi.twitch.tv PRIVMSG #calledude :hej
        private void decodeMessage()
        {
            //Get name of sender
            var indexUpper = message.IndexOf('!');
            var indexLower = message.IndexOf(':') + 1;
            var nameLength = indexUpper - indexLower;

            var name = message.Substring(indexLower, nameLength);
            Sender = new User(char.ToUpper(name.First()) + name.Substring(1).ToLower()); //capitalize first letter in username

            //Get content
            var stringFormatted = new string[message.Split(' ').Length - 3];
            var counter = 0;

            for (var i = 3; i < message.Split(' ').Length; i++)
            {
                stringFormatted[counter] = message.Split(' ')[i];
                counter++;
            }
            Content = string.Join(" ", stringFormatted).Substring(1); //Deletes ":" from the first letter in the message
        }
    }
}