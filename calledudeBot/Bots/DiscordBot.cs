using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using calledudeBot.Chat;
using System.Collections.Generic;
using OBSWebsocketDotNet;
using System.Timers;
using System.Diagnostics;

namespace calledudeBot.Bots
{
    public sealed class DiscordBot : Bot<DiscordMessage>
    {
        private DiscordSocketClient bot;
        private readonly MessageHandler<DiscordMessage> messageHandler;
        private DateTime streamStarted;
        private readonly ulong announceChanID, streamerID;
        private bool isStreaming;
        private readonly Timer streamStatusTimer;
        private SocketUser streamer;
        private OBSWebsocket obs;

        public DiscordBot(string token, ulong announceChanID, ulong streamerID) 
            : base("Discord")
        {
            Token = token;
            this.announceChanID = announceChanID;
            this.streamerID = streamerID;
            messageHandler = new MessageHandler<DiscordMessage>(this);

            streamStatusTimer = new Timer(2000);
            streamStatusTimer.Elapsed += checkStreamStatus;
        }

        internal override async Task Start()
        {
            bot = new DiscordSocketClient();
            if (!testRun)
            {
                bot.MessageReceived += onMessageReceived;
                bot.Connected += onConnect;
                bot.Ready += onReady;
                bot.Disconnected += onDisconnect;
            }

            await bot.LoginAsync(TokenType.Bot, Token);
            await bot.StartAsync();
        }

        private async Task onReady()
        {
            streamer = bot.GetUser(streamerID);

            obs = new OBSWebsocket();
            obs.WSTimeout = TimeSpan.FromSeconds(5);
            obs.StreamingStateChanged += toggleLiveStatus;

            await connectToOBS();
        }

        private async Task connectToOBS()
        {
            TryLog("Waiting for OBS to start.");
            List<Process> procs = null;
            while (procs is null || !procs.Any())
            {
                procs = Process.GetProcessesByName("obs32")
                        .Concat(Process.GetProcessesByName("obs64"))
                        .ToList();
                await Task.Delay(500);
            }

            //Trying 5 times just in case.
            if (Enumerable.Range(1, 5).Select(_ => obs.Connect("ws://localhost:4444")).All(x => !x))
            {
                TryLog("You need to install the obs-websocket plugin for OBS and configure it to run on port 4444.");
                await Task.Delay(3000);
                Process.Start("https://github.com/Palakis/obs-websocket/releases");
                await Task.Delay(10000);
                await connectToOBS();
            }
            else
            {
                TryLog("Connected to OBS. Start streaming!");

                var obsProc = procs[0];
                obsProc.EnableRaisingEvents = true;
                obsProc.Exited += onObsExit;
            }
        }

        private Task onConnect()
        {
            TryLog("Connected to Discord.");
            return Task.CompletedTask;
        }

        private Task onDisconnect(Exception e)
        {
            TryLog("Disconnected from Discord.");
            return Task.CompletedTask;
        }

        private Task onMessageReceived(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message or if we sent it ourselves
            var message = messageParam as SocketUserMessage;
            if (message == null || bot.CurrentUser.Id == message.Author.Id) return Task.CompletedTask;

            DiscordMessage msg = new DiscordMessage(message.Content)
            {
                Sender = new DiscordUser(message.Author),
                Destination = message.Channel.Id
            };
            messageHandler.DetermineResponse(msg);

            return Task.CompletedTask;
        }

        public bool IsMod(SocketGuildUser user)
        {
            return user.GuildPermissions.BanMembers || user.GuildPermissions.KickMembers;
        }

        public IEnumerable<SocketGuildUser> GetModerators()
        {
            var channel = bot.GetChannel(announceChanID) as IGuildChannel;
            var roles = channel.Guild.Roles as IReadOnlyCollection<SocketRole>;
            return roles.Where(x => x.Permissions.BanMembers || x.Permissions.KickMembers).SelectMany(r => r.Members);
        }

        public override async void SendMessage(DiscordMessage message)
        {
            var channel = bot.GetChannel(message.Destination) as IMessageChannel;
            await channel.SendMessageAsync(message.Content);
        }

        private async void onObsExit(object sender, EventArgs e)
        {
            isStreaming = false;
            streamStatusTimer.Stop();
            obs.Disconnect();
            await connectToOBS();
        }

        private void checkStreamStatus(object sender, ElapsedEventArgs e)
        {
            if (streamer?.Activity is StreamingGame sg)
            {
                var twitchUsername = sg.Url.Split('/').Last();
                DiscordMessage msg = new DiscordMessage($"{twitchUsername} just went live with the title: \"{sg.Name}\" - Watch at: {sg.Url}")
                {
                    Destination = announceChanID
                };
                SendMessage(msg);
                isStreaming = true;
                streamStatusTimer.Stop();
            }
        }

        private void toggleLiveStatus(OBSWebsocket sender, OutputState type)
        {
            if (type == OutputState.Started)
            {
                streamStarted = DateTime.Now;
                streamStatusTimer.Start();
            }
            else if (type == OutputState.Stopped)
            {
                isStreaming = false;
                streamStatusTimer.Stop();
            }
        }
        
        public DateTime WentLiveAt()
        {
            if (isStreaming)
                return streamStarted;
            else
                return new DateTime();
        }

        internal override async void Logout()
        {
            await bot.LogoutAsync();
            await bot.StopAsync();
        }

        protected override void Dispose(bool disposing)
        {
            bot.Dispose();
            obs?.Disconnect();
            streamStatusTimer.Dispose();
        }
    }
}