﻿using calledudeBot.Chat;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

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
        protected string channelName;
        protected event Func<Task> Ready;
        protected event Action<string, string> MessageReceived;
        protected event Action<string> UnhandledMessage;

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
            => await WriteLine($"PRIVMSG {channelName} :{message.Content}");

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

        internal override Task Logout()
        {
            sock.Close();
            return Task.CompletedTask;
        }

        protected async Task Login()
        {
            await WriteLine("PASS " + Token + "\r\nNICK " + nick + "\r\n");
            int result = 0;
            for (var buf = await input.ReadLineAsync(); result != successCode; buf = await input.ReadLineAsync())
            {
                int.TryParse(buf.Split(' ')[1], out result);
                if (buf == null || result == 464
                    || (buf.StartsWith(":tmi.twitch.tv NOTICE * ") && (buf.EndsWith(":Improperly formatted auth") || buf.EndsWith(":Login authentication failed"))))
                {
                    throw new InvalidOrWrongTokenException(buf);
                }
                if (result == 001)
                {
                    if(channelName != null)
                        await WriteLine($"JOIN {channelName}");

                    if (Ready != null)
                        await Ready.Invoke();

                    if (!TestRun)
                    {
                        TryLog($"Connected to {Name}-IRC.");
                    }
                }
            }
        }

        protected virtual async Task Listen()
        {
            while (true)
            {
                var buffer = await input.ReadLineAsync();
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
            => await output.WriteLineAsync(message);

        protected override void Dispose(bool disposing)
        {
            sock.Dispose();
            input.Dispose();
            output.Dispose();
        }
    }
}
