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
            _bot = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info
            });

            if (!TestRun)
            {
                _bot.Log += (e) =>
                {
                    TryLog($"{e.Message}.");
                    return Task.CompletedTask;
                };
                _bot.MessageReceived += OnMessageReceived;
                _bot.Ready += OnReady;
            }

            await _bot.LoginAsync(TokenType.Bot, Token);
            await _bot.StartAsync();
        }

        private async Task OnReady()
        {
            _streamMonitor = await StreamMonitor.Create(_announceChanID, _streamerID, _bot);
            _ = _streamMonitor.Connect();
        }

        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message or if we sent it ourselves
            if (!(messageParam is SocketUserMessage message)
                || _bot.CurrentUser.Id == message.Author.Id
                || !(message.Author is SocketGuildUser user))
            {
                return;
            }

            var isMod = GetModerators()
                .Any(u => u.Id == user.Id)
                || user.GuildPermissions.BanMembers
                || user.GuildPermissions.KickMembers;

            DiscordMessage msg = new DiscordMessage(
                message.Content,
                new User($"{user.Username}#{user.Discriminator}", isMod),
                message.Channel.Id);

            await _messageHandler.DetermineResponse(msg);
        }

        private IReadOnlyCollection<SocketGuildUser> GetModerators()
        {
            var channel = _bot.GetChannel(_announceChanID) as IGuildChannel;
            var roles = channel.Guild.Roles.Cast<SocketRole>();
            return roles
                .Where(x => x.Permissions.BanMembers || x.Permissions.KickMembers)
                .SelectMany(r => r.Members)
                .ToArray();
        }

        protected override async Task SendMessage(DiscordMessage message)
        {
            var channel = _bot.GetChannel(message.Destination) as IMessageChannel;
            await channel.SendMessageAsync(message.Content);
        }

        public DateTime WentLiveAt()
        {
            return _streamMonitor?.IsStreaming ?? false
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