using System.Linq;

namespace calledudeBot
{
    //IDEA: Array of some sort for rest of arguments?

    public class Message
    {
        private string message;
        private string user;
        private Bot creator;
        private ulong destination;

        public string Content
        {
            get { return message; }
            set { message = value; }
        }
        public string Sender
        {
            get { return user; }
            set { user = value; }
        }
        public Bot Origin
        {
            get { return creator; }
        }
        public ulong Destination
        {
            get { return destination; }
            set { destination = value; }
        }

        public Message(string message)
        {
            this.message = message;
        }

        public Message(string message, Bot bot)
        {
            creator = bot;
            this.message = message;
            if(typeof(TwitchBot) == bot.GetType())
            {
                decodeMessage();
            }
        }


        //:calledude!calledude@calledude.tmi.twitch.tv PRIVMSG #calledude :hej
        void decodeMessage()
        {
            //Get name of sender
            var indexUpper = message.IndexOf('!');
            var indexLower = message.IndexOf(':') + 1;
            var nameLength = indexUpper - indexLower;

            var name = message.Substring(indexLower, nameLength);
            user = char.ToUpper(name.First()) + name.Substring(1).ToLower(); //capitalize first letter in username

            //Get content
            var stringFormatted = new string[message.Split(' ').Length - 3];
            var counter = 0;

            for (var i = 3; i < message.Split(' ').Length; i++)
            {
                stringFormatted[counter] = message.Split(' ')[i];
                counter++;
            }

            message = string.Join(" ", stringFormatted).Substring(1); //Deletes ":" from the first letter in the message

        }
    }
}