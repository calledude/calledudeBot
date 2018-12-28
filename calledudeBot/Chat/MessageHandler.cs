using calledudeBot.Bots;
using calledudeBot.Chat.Info;
using calledudeBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace calledudeBot.Chat
{
    public class MessageHandler
    {
        private OsuBot osu;
        private Bot bot;
        private Queue<Message> messageQueue = new Queue<Message>();
        private DateTime lastMessage;
        private Timer relayTimer;
        private string osuAPIToken, streamerNick;
        private CommandHandler commandHandler;
        private string songRequestLink = "https://osu.ppy.sh/api/get_beatmaps?k={0}&b={1}";

        public MessageHandler(Bot bot)
        {
            if (Bot.testRun) return;
            commandHandler = new CommandHandler(this);
            this.bot = bot;
        }

        public MessageHandler(Bot bot, string streamerNick, string osuAPIToken) : this(bot)
        {
            if (bot is TwitchBot)
            {
                osu = calledudeBot.osuBot;
                this.osuAPIToken = osuAPIToken;
                this.streamerNick = streamerNick.Substring(1).ToLower();
                relayTimer = new Timer(200);
                relayTimer.Elapsed += tryRelay;
                relayTimer.Start();
            }
        }

        public void determineResponse(Message message)
        {
            var msg = message.Content.Split(' ');
            var cmd = msg.First();

            if (commandHandler.isPrefixed(cmd))
            {
                var param = new CommandParameter(msg, message);
                respond(commandHandler.getResponse(param));
            }
            else
            {
                if (message.Content.Contains("://osu.ppy.sh/b/"))
                {
                    requestSong(message);
                }
                if (message.Origin is TwitchBot && message.Sender.Name.ToLower() != streamerNick) //We only want to relay messages from twitch
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

        private void respond(Message message)
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
            var idx = message.Content.IndexOf("/b/") + "/b/".Length;
            var num = message.Content.Skip(idx).TakeWhile(c => char.IsNumber(c));
            var beatmapID = string.Concat(num);
            var reqLink = string.Format(songRequestLink, osuAPIToken, beatmapID);

            APIHandler api = new APIHandler(reqLink, RequestData.OsuSong);
            JsonData data = api.requestOnce();
            if (data?.osuSongData?.Count > 0)
            {
                OsuSongData o = data.osuSongData[0];
                message.Content = $"[http://osu.ppy.sh/b/{beatmapID} {o.artist} - {o.title} [{o.version}]]";
            }
        }
    }
}