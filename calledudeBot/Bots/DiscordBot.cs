using calledudeBot.Chat;
using calledudeBot.Config;
using calledudeBot.Services;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public sealed class DiscordBot : Bot<DiscordMessage>
    {
        private readonly DiscordSocketClient _bot;
        private readonly ulong _announceChanID, _streamerID;
        private readonly StreamMonitor _streamMonitor;
        private readonly MessageDispatcher _dispatcher;

        protected override string Token { get; }

        public DiscordBot(
            BotConfig config,
            StreamMonitor streamMonitor,
            DiscordSocketClient bot,
            MessageDispatcher dispatcher)
            : base("Discord")
        {
            _bot = bot;
            Token = config.DiscordToken;

            _announceChanID = config.AnnounceChannelId;
            _streamerID = config.StreamerId;
            _streamMonitor = streamMonitor;
            _dispatcher = dispatcher;
        }

        public override async Task Start()
        {
            _bot.Log += (e) =>
            {
                Log($"{e.Message}.");
                return Task.CompletedTask;
            };
            _bot.MessageReceived += OnMessageReceived;
            _bot.Ready += OnReady;

            await _bot.LoginAsync(TokenType.Bot, Token);
            await _bot.StartAsync();
        }

        private Task OnReady()
        {
            _ = _streamMonitor.Connect();

            return Task.CompletedTask;
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
                $"#{message.Channel.Name}",
                new User($"{user.Username}#{user.Discriminator}", isMod),
                message.Channel.Id);

            await _dispatcher.PublishAsync(msg);
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

        public override async Task Logout()
        {
            await _bot.LogoutAsync();
            await _bot.StopAsync();
        }

        public override void Dispose(bool disposing)
        {
            _bot.Dispose();
            _streamMonitor?.Dispose();
        }
    }
}