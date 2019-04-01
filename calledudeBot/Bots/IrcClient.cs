using calledudeBot.Chat;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public abstract class IrcClient : Bot<IrcMessage>
    {
        private readonly int _successCode;
        private TcpClient _sock;
        private StreamWriter _output;
        private StreamReader _input;
        protected string Nick;
        protected int Port = 6667;
        protected string Server;
        protected string ChannelName;
        protected event Func<Task> Ready;
        protected event Action<string, string> MessageReceived;
        protected event Action<string> UnhandledMessage;

        protected IrcClient(string server, string name, int successCode) : base(name)
        {
            Server = server;
            _successCode = successCode;
            Setup();
        }

        private void Setup()
        {
            _sock = new TcpClient();
            _sock.Connect(Server, Port);
            _output = new StreamWriter(_sock.GetStream());
            _output.AutoFlush = true;
            _input = new StreamReader(_sock.GetStream());
        }

        internal override async Task Start()
        {
            await Login();

            if (!TestRun)
            {
                try
                {
                    await Listen();
                }
                catch (Exception e)
                {
                    TryLog(e.Message);
                    Reconnect(); //Since basically any exception will break the fuck out of the bot, reconnect
                }
            }
        }

        protected async Task SendPong(string ping)
        {
            string pong = ping.Replace("PING", "PONG");
            await WriteLine(pong);
            TryLog(pong);
        }

        protected override async Task SendMessage(IrcMessage message)
            => await WriteLine($"PRIVMSG {ChannelName} :{message.Content}");

        protected async void Reconnect()
        {
            TryLog($"Disconnected. Re-establishing connection..");
            Dispose(true);

            while (!_sock.Connected)
            {
                Setup();
                await Start();
                await Task.Delay(5000);
            }
        }

        internal override Task Logout()
        {
            _sock.Close();
            return Task.CompletedTask;
        }

        protected async Task Login()
        {
            await WriteLine("PASS " + Token + "\r\nNICK " + Nick + "\r\n");
            int result = 0;
            for (var buf = await _input.ReadLineAsync(); result != _successCode; buf = await _input.ReadLineAsync())
            {
                int.TryParse(buf.Split(' ')[1], out result);
                if (buf == null || result == 464
                    || (buf.StartsWith(":tmi.twitch.tv NOTICE * ") && (buf.EndsWith(":Improperly formatted auth") || buf.EndsWith(":Login authentication failed"))))
                {
                    throw new InvalidOrWrongTokenException(buf);
                }
                if (result == 001)
                {
                    if(ChannelName != null)
                        await WriteLine($"JOIN {ChannelName}");

                    if (Ready != null)
                        await Ready.Invoke();

                    if (!TestRun)
                    {
                        TryLog($"Connected to {Name}-IRC.");
                    }
                }
            }
        }

        protected async Task Listen()
        {
            while (true)
            {
                var buffer = await _input.ReadLineAsync();
                var b = buffer.Split(' ');

                if (b[0] == "PING")
                {
                    await SendPong(buffer);
                }
                else if (b[1] == "PRIVMSG")
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

        protected override void Dispose(bool disposing)
        {
            _sock.Dispose();
            _input.Dispose();
            _output.Dispose();
        }
    }
}
