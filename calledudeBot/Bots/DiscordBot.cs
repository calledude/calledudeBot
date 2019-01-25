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
    public class DiscordBot : Bot
    {
        private DiscordSocketClient bot;
        private MessageHandler messageHandler;
        private DateTime streamStarted;
        private ulong announceChanID, streamerID;
        private bool isStreaming;
        private Timer streamStatusTimer;
        private SocketUser streamer;
        private OBSWebsocket obs;

        public DiscordBot(string token, string announceChanID, string streamerID) 
            : base("Discord")
        {
            Token = token;
            this.announceChanID = Convert.ToUInt64(announceChanID);
            this.streamerID = Convert.ToUInt64(streamerID);
            messageHandler = new MessageHandler(this);

            streamStatusTimer = new Timer(2000);
            streamStatusTimer.Elapsed += CheckStreamStatus;
        }

        internal override async Task Start()
        {
            bot = new DiscordSocketClient();
            if (!testRun)
            {
                bot.MessageReceived += HandleCommand;
                bot.Connected += onConnect;
                bot.Ready += Ready;
                bot.Disconnected += onDisconnect;
            }

            await bot.LoginAsync(TokenType.Bot, Token);
            await bot.StartAsync();
        }

        private async Task Ready()
        {
            streamer = bot.GetUser(streamerID);

            obs = new OBSWebsocket();
            obs.WSTimeout = TimeSpan.FromSeconds(5);
            obs.StreamingStateChanged += ToggleLiveStatus;

            await ConnectToOBS();
        }

        private async Task ConnectToOBS()
        {
            tryLog("Waiting for OBS to start.");
            List<Process> procs = null;
            while (procs == null || !procs.Any())
            {
                procs = Process.GetProcessesByName("obs32")
                        .Concat(Process.GetProcessesByName("obs64"))
                        .ToList();
                await Task.Delay(500);
            }

            //Trying 5 times just in case.
            if (Enumerable.Range(1, 5).Select(x => obs.Connect("ws://localhost:4444")).All(x => !x))
            {
                tryLog("You need to install the obs-websocket plugin for OBS and configure it to run on port 4444.");
                await Task.Delay(3000);
                Process.Start("https://github.com/Palakis/obs-websocket/releases");
                await Task.Delay(10000);
                await ConnectToOBS();
            }
            else
            {
                tryLog("Connected to OBS. Start streaming!");

                var obsProc = procs.First();
                obsProc.EnableRaisingEvents = true;
                obsProc.Exited += OnObsExit;
            }
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

        public IEnumerable<SocketGuildUser> getModerators()
        {
            var channel = bot.GetChannel(announceChanID) as IGuildChannel;
            var roles = channel.Guild.Roles as IReadOnlyCollection<SocketRole>;
            return roles.Where(x => x.Permissions.BanMembers || x.Permissions.KickMembers).SelectMany(r => r.Members);
        }

        public override async void sendMessage(Message message)
        {
            var channel = bot.GetChannel(message.Destination) as IMessageChannel;
            await channel.SendMessageAsync(message.Content);
        }

        private async void OnObsExit(object sender, EventArgs e)
        {
            isStreaming = false;
            streamStatusTimer.Stop();
            obs.Disconnect();
            await ConnectToOBS();
        }

        private void CheckStreamStatus(object sender, ElapsedEventArgs e)
        {
            if (streamer?.Activity is StreamingGame sg)
            {
                var twitchUsername = sg.Url.Split('/').Last();
                Message msg = new Message($"{twitchUsername} just went live with the title: \"{sg.Name}\" - Watch at: {sg.Url}", this)
                {
                    Destination = announceChanID
                };
                sendMessage(msg);
                isStreaming = true;
                streamStatusTimer.Stop();
            }
        }

        private void ToggleLiveStatus(OBSWebsocket sender, OutputState type)
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
        
        public DateTime wentLiveAt()
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