using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using calledudeBot.Chat;

namespace calledudeBot.Bots
{
    public abstract class IrcClient : Bot<IrcMessage>
    {
        private readonly int successCode;
        protected TcpClient sock;
        protected StreamWriter output;
        protected StreamReader input;
        protected string nick;
        protected int port = 6667;
        protected string server;
        protected string buf;
        protected string channelName;
        protected event Action OnReady;

        public abstract Task Listen();

        protected IrcClient(string server, string name, int successCode) : base(name)
        {
            this.server = server;
            this.successCode = successCode;
            Setup();
        }

        protected void Setup()
        {
            sock = new TcpClient();
            sock.Connect(server, port);
            output = new StreamWriter(sock.GetStream());
            output.AutoFlush = true;
            input = new StreamReader(sock.GetStream());
        }

        internal override async Task Start()
        {
            await Login();

            if (!testRun)
            {
                try
                {
                    await Listen();
                }
                catch (Exception e)
                {
                    TryLog(e.Message);
                    TryLog(e.StackTrace);
                    Reconnect(); //Since basically any exception will break the fuck out of the bot, reconnect
                }
            }
        }

        protected void SendPong()
        {
            string pong = buf.Replace("PING", "PONG");
            WriteLine(pong);
            TryLog(pong);
        }


        public override void SendMessage(IrcMessage message) 
            => WriteLine($"PRIVMSG {channelName} :{message.Content}");

        protected async void Reconnect()
        {
            TryLog($"Disconnected. Re-establishing connection..");
            Dispose(true);

            while (!sock.Connected)
            {
                Setup();
                await Start();
                await Task.Delay(5000);
            }
        }

        internal override void Logout() => sock.Close();

        protected async Task Login()
        {
            WriteLine("PASS " + Token + "\r\nNICK " + nick + "\r\n");
            int result = 0;
            for (buf = await input.ReadLineAsync(); result != successCode; buf = await input.ReadLineAsync())
            {
                if (buf == null || result == 464
                    || (buf.StartsWith(":tmi.twitch.tv NOTICE * ") && (buf.EndsWith(":Improperly formatted auth") || buf.EndsWith(":Login authentication failed"))))
                {
                    throw new InvalidOrWrongTokenException(buf);
                }
                if (result == 001)
                {
                    WriteLine($"JOIN {channelName}");
                    OnReady?.Invoke();
                    if (!testRun) TryLog($"Connected to {Name}-IRC.");
                }
                int.TryParse(buf.Split(' ')[1], out result);
            }
        }

        protected void WriteLine(string message) 
            => output.WriteLine(message);

        protected override void Dispose(bool disposing)
        {
            sock.Dispose();
            input.Dispose();
            output.Dispose();
        }
    }
}
