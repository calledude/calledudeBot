using Discord;
using Discord.WebSocket;
using OBSWebsocketDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace calledudeBot.Services
{
    public class StreamMonitor : IDisposable
    {
        private readonly IGuildUser _streamer;
        private readonly OBSWebsocket _obs;
        private readonly Timer _streamStatusTimer;
        private readonly DiscordSocketClient _client;
        private readonly ITextChannel _announceChannel;

        public bool IsStreaming { get; private set; }
        public DateTime StreamStarted { get; private set; }

        public StreamMonitor(ulong streamerID, ulong announceChanID, DiscordSocketClient client)
        {
            _client = client;
            _obs = new OBSWebsocket();
            _obs.WSTimeout = TimeSpan.FromSeconds(5);
            _obs.StreamStatus += CheckLiveStatus;

            _streamStatusTimer = new Timer(2000);
            _streamStatusTimer.Elapsed += CheckDiscordStatus;

            _announceChannel = _client.GetChannel(announceChanID) as ITextChannel;
            _streamer = _announceChannel.Guild.GetUserAsync(streamerID)
                                            .GetAwaiter().GetResult();
        }

        private void Log(string message)
        {
            Logger.Log($"[StreamMonitor]: {message}");
        }

        public async Task Connect()
        {
            Log("Waiting for OBS to start.");
            List<Process> procs = null;
            while (procs is null || !procs.Any())
            {
                procs = Process.GetProcessesByName("obs32")
                        .Concat(Process.GetProcessesByName("obs64"))
                        .ToList();
                await Task.Delay(2000);
            }

            //Trying 5 times just in case.
            if (Enumerable.Range(1, 5).Select(_ => _obs.Connect("ws://localhost:4444")).All(x => !x))
            {
                Log("You need to install the obs-websocket plugin for OBS and configure it to run on port 4444.");
                await Task.Delay(3000);
                Process.Start("https://github.com/Palakis/obs-websocket/releases");
                await Task.Delay(10000);
                await Connect();
            }
            else
            {
                Log("Connected to OBS. Start streaming!");

                var obsProc = procs[0];
                obsProc.EnableRaisingEvents = true;
                obsProc.Exited += OnObsExit;
            }
        }

        private async void CheckDiscordStatus(object sender, ElapsedEventArgs e)
        {
            if (_streamer?.Activity is StreamingGame sg)
            {
                _streamStatusTimer.Stop();
                IsStreaming = true;

                var messages = await _announceChannel
                    .GetMessagesAsync()
                    .FlattenAsync();
                
                var twitchUsername = sg.Url.Split('/').Last();
                var msg = $"🔴 **{twitchUsername}** is now **LIVE**\n- Title: **{sg.Name}**\n- Watch at: {sg.Url}";

                //StreamStarted returns the _true_ time that the stream started
                //If any announcement message exists within 3 minutes of that, don't send a new announcement
                //In that case we assume that the bot has been restarted (for whatever reason)
                if (!messages.Any(m =>
                    m.Author.Id == _client.CurrentUser.Id
                    && m.Content == msg
                    && StreamStarted - m.Timestamp < TimeSpan.FromMinutes(3)))
                {
                    await _announceChannel.SendMessageAsync(msg);
                }
            }
        }

        private async void OnObsExit(object sender, EventArgs e)
        {
            IsStreaming = false;
            _streamStatusTimer.Stop();
            _obs.Disconnect();
            await Connect();
        }

        private void CheckLiveStatus(OBSWebsocket sender, StreamStatus status)
        {
            if (status.Streaming == IsStreaming)
            {
                return;
            }

            if (status.Streaming)
            {
                StreamStarted = DateTime.Now - status.TotalStreamTime;
                _streamStatusTimer.Start();
            }
            else
            {
                IsStreaming = false;
                _streamStatusTimer.Stop();
            }
        }

        public void Dispose()
        {
            _streamStatusTimer.Dispose();
            _obs.Disconnect();
        }
    }
}

