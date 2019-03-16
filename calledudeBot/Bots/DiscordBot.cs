using calledudeBot.Chat;
using calledudeBot.Services;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public sealed class DiscordBot : Bot<DiscordMessage>
    {
        private DiscordSocketClient _bot;
        private readonly MessageHandler<DiscordMessage> _messageHandler;
        private readonly ulong _announceChanID, _streamerID;
        private StreamMonitor _streamMonitor;

        public DiscordBot(string token, ulong announceChanID, ulong streamerID) 
            : base("Discord")
        {
            Token = token;
            _announceChanID = announceChanID;
            _streamerID = streamerID;
            _messageHandler = new MessageHandler<DiscordMessage>(this);
        }

        internal override async Task Start()
        {
            _bot = new DiscordSocketClient();
            if (!testRun)
            {
                _bot.MessageReceived += OnMessageReceived;
                _bot.Connected += OnConnect;
                _bot.Ready += OnReady;
                _bot.Disconnected += OnDisconnect;
            }

            await _bot.LoginAsync(TokenType.Bot, Token);
            await _bot.StartAsync();
        }

        private async Task OnReady()
        {
            _streamMonitor = new StreamMonitor(_streamerID, _announceChanID, _bot);
            await _streamMonitor.Connect();
        }

        private Task OnConnect()
        {
            TryLog("Connected to Discord.");
            return Task.CompletedTask;
        }

        private Task OnDisconnect(Exception e)
        {
            TryLog("Disconnected from Discord.");
            return Task.CompletedTask;
        }

        private Task OnMessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message or if we sent it ourselves
            var message = messageParam as SocketUserMessage;
            if (message == null || _bot.CurrentUser.Id == message.Author.Id)
            {
                return Task.CompletedTask;
            }

            DiscordMessage msg = new DiscordMessage(message.Content)
            {
                Sender = new DiscordUser(message.Author),
                Destination = message.Channel.Id
            };
            _messageHandler.DetermineResponse(msg);

            return Task.CompletedTask;
        }

        public bool IsMod(SocketGuildUser user)
        {
            return user.GuildPermissions.BanMembers || user.GuildPermissions.KickMembers;
        }

        public IEnumerable<SocketGuildUser> GetModerators()
        {
            var channel = _bot.GetChannel(_announceChanID) as IGuildChannel;
            var roles = channel.Guild.Roles as IReadOnlyCollection<SocketRole>;
            return roles.Where(x => x.Permissions.BanMembers || x.Permissions.KickMembers).SelectMany(r => r.Members);
        }

        public override async void SendMessage(DiscordMessage message)
        {
            var channel = _bot.GetChannel(message.Destination) as IMessageChannel;
            await channel.SendMessageAsync(message.Content);
        }
        
        public DateTime WentLiveAt()
        {
            return _streamMonitor.IsStreaming 
                ? _streamMonitor.StreamStarted 
                : default;
        }

        internal override async Task Logout()
        {
            await _bot.LogoutAsync();
            await _bot.StopAsync();
        }

        protected override void Dispose(bool disposing)
        {
            _bot.Dispose();
            _streamMonitor?.Dispose();
        }
    }
}