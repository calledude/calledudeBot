using calledudeBot.Bots;
using calledudeBot.Services;
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
        private string osuAPIToken = calledudeBot.osuAPIToken;

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
            if (status == CommandStatus.NotHandled)
            {
                if (message.Content.Split(' ')[0].Contains("://osu.ppy.sh/b/"))
                {
                    requestSong(message);
                }
                if (message.Origin == bot) //We only want to relay messages from twitch
                {
                    messageQueue.Enqueue(message);
                    tryRelay(null, null);
                }
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

        //[http://osu.ppy.sh/b/795232 fhana - Wonder Stella [Stella]]
        private void requestSong(Message message)
        {
            string beatmapID = message.Content.Split('/')[4];

            APIHandler api = new APIHandler($"https://osu.ppy.sh/api/get_beatmaps?k={osuAPIToken}&b={beatmapID}", RequestData.OsuSong);
            JsonData data = api.requestOnce();
            if (data?.osuSongData?.Count > 0)
            {
                OsuSongData o = data.osuSongData[0];
                message.Content = "[http://osu.ppy.sh/b/" + beatmapID + " " + o.artist + " - " + o.title + " [" + o.version + "]]";
            }
        }
    }
}