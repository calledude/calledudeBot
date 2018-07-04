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
        private MessageHandler messageHandler;
        private DateTime streamStarted;
        private ulong announceChanID;
        private bool isOnline;
        private string twitchUsername, twitchAPItoken;

        public DiscordBot(string token, string twitchAPItoken, string channelName, string announceChanID)
        {
            instanceName = "Discord";
            this.token = token;
            this.twitchAPItoken = twitchAPItoken;
            this.announceChanID = Convert.ToUInt64(announceChanID);
            twitchUsername = channelName.Substring(1);
            messageHandler = new MessageHandler(this);

            string url = $"https://api.twitch.tv/helix/streams?user_login={twitchUsername}";
            api = new APIHandler(url, RequestData.TwitchUser, twitchAPItoken);
            api.DataReceived += determineLiveStatus;
        }

        public override async Task Start()
        {
            bot = new DiscordSocketClient();
            if (!testRun)
            {
                bot.MessageReceived += HandleCommand;
                bot.Connected += onConnect;
                bot.Disconnected += onDisconnect;
            }

            await bot.LoginAsync(TokenType.Bot, token);
            await bot.StartAsync();
            while(bot.ConnectionState != ConnectionState.Connected) { }
        }

        private Task onConnect()
        {
            tryLog("Connected to Discord.");
            return Task.CompletedTask;
        }

        private Task onDisconnect(Exception e)
        {
            tryLog("Disconnected from Discord.");
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

        public override async void sendMessage(Message message)
        {
            var channel = bot.GetChannel(message.Destination) as IMessageChannel;
            await channel.SendMessageAsync(message.Content);
        }

        private void determineLiveStatus(JsonData jsonData)
        {
            if(jsonData?.twitchData?.Count > 0)
            {
                if (!isOnline)
                {
                    TwitchData data = jsonData.twitchData[0];
                    Message msg = new Message($"{twitchUsername} just went live with the title: \"{data.title}\" - Watch at: https://twitch.tv/{twitchUsername}/", this)
                    {
                        Destination = announceChanID
                    };
                    sendMessage(msg);

                    streamStarted = data.started_at.ToLocalTime();
                    isOnline = true;
                }
            }
            else
            {
                isOnline = false;
            }
        }

        public DateTime wentLiveAt()
        {
            if (isOnline)
                return streamStarted;
            else
                return new DateTime();
        }

        protected override async void Logout()
        {
            await bot.LogoutAsync();
            await bot.StopAsync();
        }
    }
}