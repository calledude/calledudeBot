using calledudeBot.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public abstract class IrcClient : Bot<IrcMessage>
    {
        public string Nick { get; }
        public string ChannelName { get; }

        protected abstract List<string> Failures { get; }
        protected event Func<Task> Ready;
        protected event Action<string, string> MessageReceived;
        protected event Action<string> UnhandledMessage;

        private readonly string _server;
        private readonly int _port;
        private readonly int _successCode;
        private TcpClient _sock;
        private StreamWriter _output;
        private StreamReader _input;

        protected IrcClient(
            string server,
            int successCode,
            string nick,
            string channelName = null,
            int port = 6667)
        {
            Nick = nick;
            ChannelName = channelName;

            _port = port;
            _server = server;
            _successCode = successCode;

            Setup();
        }

        private void Setup()
        {
            _sock = new TcpClient();
            _sock.Connect(_server, _port);
            _output = new StreamWriter(_sock.GetStream());
            _output.AutoFlush = true;
            _input = new StreamReader(_sock.GetStream());
        }

        public override async Task Start()
        {
            var waitTask = Task.Delay(5000);
            var loginTask = Login();

            var completedTask = await Task.WhenAny(waitTask, loginTask);

            if (loginTask.IsFaulted)
            {
                Log("Login failed. Are you sure your credentials are correct?");
                throw loginTask.Exception.InnerException;
            }
            else if (completedTask != loginTask)
            {
                Log("Login timed out. Are you sure your credentials are correct?");
                throw new TimeoutException();
            }

            try
            {
                await Listen();
            }
            catch (Exception e)
            {
                Log(e.Message);
                await Reconnect(); //Since basically any exception will break the fuck out of the bot, reconnect
            }
        }

        protected async Task SendPong(string ping)
        {
            await WriteLine(ping.Replace("PING", "PONG"));
            Log($"Heartbeat sent.");
        }

        protected override async Task SendMessage(IrcMessage message)
            => await WriteLine($"PRIVMSG {ChannelName} :{message.Content}");

        protected async Task Reconnect()
        {
            Log("Disconnected. Re-establishing connection..");
            Dispose(true);

            while (!_sock.Connected)
            {
                Dispose(true);
                Setup();
                _ = Start();
                await Task.Delay(5000);
            }
        }

        public override Task Logout()
        {
            _sock.Close();
            return Task.CompletedTask;
        }

        private bool IsFailure(string buffer)
        {
            return Failures?.Any(x => x.Equals(buffer)) == true;
        }

        protected async Task Login()
        {
            await WriteLine("PASS " + Token + "\r\nNICK " + Nick + "\r\n");
            int resultCode = 0;

            while (resultCode != _successCode)
            {
                var buffer = await _input.ReadLineAsync();

                int.TryParse(buffer?.Split(' ')[1], out resultCode);
                if (buffer == null || IsFailure(buffer))
                {
                    throw new InvalidOrWrongTokenException();
                }
                else if (resultCode == 001)
                {
                    if (ChannelName != null)
                        await WriteLine($"JOIN {ChannelName}");

                    if (Ready != null)
                        await Ready.Invoke();

                    Log($"Connected to {Name}-IRC.");
                }
            }
        }

        protected async Task Listen()
        {
            while (true)
            {
                var buffer = await _input.ReadLineAsync();
                var splitBuffer = buffer.Split(' ');

                if (splitBuffer[0].Equals("PING"))
                {
                    await SendPong(buffer);
                }
                else if (splitBuffer[1].Equals("PRIVMSG"))
                {
                    var parsedMessage = IrcMessage.ParseMessage(buffer);
                    var parsedUser = IrcMessage.ParseUser(buffer);
                    MessageReceived?.Invoke(parsedMessage, parsedUser);
                }
                else
                {
                    UnhandledMessage?.Invoke(buffer);
                }
            }
        }

        protected async Task WriteLine(string message)
            => await _output.WriteLineAsync(message);

        public override void Dispose(bool disposing)
        {
            _sock.Dispose();
            _input.Dispose();
            _output.Dispose();
        }
    }
}
