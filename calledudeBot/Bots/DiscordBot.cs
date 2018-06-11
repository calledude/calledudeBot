using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using calledudeBot.Chat;
using calledudeBot.Services;

namespace calledudeBot.Bots
{
    public class DiscordBot : Bot
    {
        private DiscordSocketClient bot;
        private ulong announceChanID = calledudeBot.announceChanID;
        private bool online;
        private string twitchUsername = calledudeBot.channelName.Substring(1);
        private MessageHandler messageHandler;
        private DateTime streamStarted;
        private APIHandler api;

        public async Task Start(string token, string twitchAPItoken)
        {
            string url = $"https://api.twitch.tv/helix/streams?user_login={twitchUsername}";
            api = new APIHandler(url, RequestData.TwitchUser, twitchAPItoken);
            bot = new DiscordSocketClient();

            api.DataReceived += determineLiveStatus;

            bot.MessageReceived += HandleCommand;
            bot.Connected += onConnected;
            messageHandler = new MessageHandler(this);

            await bot.LoginAsync(TokenType.Bot, token);
            await bot.StartAsync();
            
            await Task.Delay(-1);
        }

        private Task onConnected()
        {
            Console.WriteLine($"[Discord]: Connected to Discord.");
            api.Start();
            return Task.CompletedTask;
        }

        private Task HandleCommand(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message or if we sent it ourselves
            var message = messageParam as SocketUserMessage;
            if (message == null || bot.CurrentUser.Id == message.Author.Id) return Task.CompletedTask;

            Message msg = new Message(message.Content, this)
            {
                Sender = new User(message.Author),
                Destination = message.Channel.Id
            };
            messageHandler.determineResponse(msg);

            return Task.CompletedTask;
        }

        public SocketRole getAdminRole()
        {
            var s = bot.GetGuild(announceChanID).Roles;

            foreach(SocketRole role in s)
            {
                if(role.Permissions.BanMembers || role.Permissions.KickMembers)
                {
                    return role;
                }
            }
            return null;
        }

        public override void sendMessage(Message message)
        {
            var channel = bot.GetChannel(message.Destination) as IMessageChannel;

            channel.SendMessageAsync(message.Content);
        }

        private void determineLiveStatus(JsonData jsonData)
        {
            if(jsonData?.twitchData?.Count > 0)
            {
                if (!online)
                {
                    TwitchData data = jsonData.twitchData[0];
                    Message msg = new Message($"{twitchUsername} just went live with the title: \"{data.title}\" - Watch at: https://twitch.tv/{twitchUsername}/", this)
                    {
                        Destination = announceChanID
                    };
                    sendMessage(msg);

                    streamStarted = data.started_at.ToLocalTime();
                    online = true;
                }
            }
            else
            {
                online = false;
            }
        }

        public DateTime wentLiveAt()
        {
            if (online)
                return streamStarted;
            else
                return new DateTime();
        }
    }
}