
namespace calledudeBot
{
    public class MessageHandler : Handler
    {
        private OsuBot osu;
        private Bot bot;

        public MessageHandler(Bot bot)
        {
            this.bot = bot;
            if (typeof(TwitchBot) == bot.GetType())
            {
                osu = calledudeBot.osuBot;
            }
            
            commandHandler = new CommandHandler(this);
        }

        public void determineResponse(Message message)
        {
            var status = commandHandler.determineCommand(message);

            if (status == CommandStatus.NotHandled && osu != null) //if osu isnt null, then twitchBot is the caller.
            {
                relay(message);
            }

        }

        public void respond(Message message)
        {
            bot.sendMessage(message);
        }

        private void relay(Message message)
        {
            message.Content = $"{message.Sender}: {message.Content}"; 
            osu.sendMessage(message);
        }
    }
}