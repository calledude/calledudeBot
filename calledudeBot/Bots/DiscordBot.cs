using calledudeBot.Chat;
using calledudeBot.Config;
using calledudeBot.Models;
using calledudeBot.Services;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public sealed class DiscordBot : Bot<DiscordMessage>
    {
        private readonly DiscordSocketClient _bot;
        private readonly ulong _announceChanID;
        private readonly MessageDispatcher _dispatcher;
        private readonly ILogger<DiscordBot> _logger;
        protected override string Token { get; }

        public DiscordBot(
            ILogger<DiscordBot> logger,
            BotConfig config,
            DiscordSocketClient bot,
            MessageDispatcher dispatcher) : base(logger)
        {
            _logger = logger;
            _bot = bot;
            Token = config.DiscordToken;

            _announceChanID = config.AnnounceChannelId;
            _dispatcher = dispatcher;
        }

        public override async Task Start()
        {
            _bot.Log += Log;
            _bot.MessageReceived += OnMessageReceived;
            _bot.Ready += OnReady;

            await _bot.LoginAsync(TokenType.Bot, Token);
            await _bot.StartAsync();
        }

        private Task Log(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(message.Exception, message.Message ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Error:
                    _logger.LogError(message.Exception, message.Message ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(message.ToString(prependTimestamp: false));
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(message.ToString(prependTimestamp: false));
                    break;
            }

            return Task.CompletedTask;
        }

        private async Task OnReady()
            => await _dispatcher.PublishAsync(new ReadyNotification(this));

        private async Task OnMessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message or if we sent it ourselves
            if (!(messageParam is SocketUserMessage message)
                || _bot.CurrentUser.Id == message.Author.Id
                || !(message.Author is SocketGuildUser user))
            {
                return;
            }

            var msg = new DiscordMessage(
                message.Content,
                $"#{message.Channel.Name}",
                new User($"{user.Username}#{user.Discriminator}", () => IsMod(user)),
                message.Channel.Id);

            await _dispatcher.PublishAsync(msg);
        }

        private Task<bool> IsMod(SocketGuildUser user)
        {
            _logger.LogInformation("Getting moderators");
            var channel = _bot.GetChannel(_announceChanID) as IGuildChannel;
            var roles = channel.Guild.Roles.Cast<SocketRole>();
            var moderatorUsers = roles
                .Where(x => x.Permissions.BanMembers || x.Permissions.KickMembers)
                .SelectMany(r => r.Members);

            var isMod = moderatorUsers
                .Any(u => u.Id == user.Id)
                || user.GuildPermissions.BanMembers
                || user.GuildPermissions.KickMembers;

            return Task.FromResult(isMod);
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
            => _bot.Dispose();
    }
}