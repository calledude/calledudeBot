using calledudeBot.Bots;
using System;
using System.Collections.Generic;
using System.Timers;

namespace calledudeBot.Chat
{
    public class MessageHandler : Handler
    {
        private OsuBot osu;
        private Bot bot;
        private Queue<Message> messageQueue = new Queue<Message>();
        private DateTime lastMessage;
        private Timer relayTimer;

        public MessageHandler(Bot bot)
        {
            commandHandler = new CommandHandler(this);
            this.bot = bot;
            if (typeof(TwitchBot) == bot.GetType())
            {
                commandHandler.init();
                osu = calledudeBot.osuBot;
                relayTimer = new Timer(200);
                relayTimer.Elapsed += tryRelay;
                relayTimer.Start();
            }
        }

        public void determineResponse(Message message)
        {
            var status = commandHandler.determineCommand(message);

            if (status == CommandStatus.NotHandled && osu != null) //if osu isnt null, then twitchBot is the caller.
            {
                messageQueue.Enqueue(message);
                tryRelay(null, null);
            }

        }

        private void tryRelay(object sender, ElapsedEventArgs e)
        {
            if (DateTime.Now - lastMessage > TimeSpan.FromMilliseconds(500) && messageQueue.Count > 0)
            {
                relay(messageQueue.Dequeue());
                lastMessage = DateTime.Now;
            }
        }

        public void respond(Message message)
        {
            bot.sendMessage(message);
        }

        private void relay(Message message)
        {
            message.Content = $"{message.Sender.Name}: {message.Content}"; 
            osu.sendMessage(message);
        }
    }
}