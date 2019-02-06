﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using calledudeBot.Chat;

namespace calledudeBot.Bots
{
    public abstract class IrcClient : Bot<IrcMessage>
    {
        protected TcpClient sock;
        protected StreamWriter output;
        protected StreamReader input;
        protected string nick;
        protected int port = 6667;
        protected string server;
        protected string buf;
        protected string channelName;

        public abstract void Listen();

        protected IrcClient(string server, string name) : base(name)
        {
            this.server = server;
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

        internal override Task Start()
        {
            Login();

            if (!testRun)
            {
                try
                {
                    Listen();
                }
                catch (Exception e)
                {
                    TryLog(e.Message);
                    TryLog(e.StackTrace);
                    Reconnect(); //Since basically any exception will break the fuck out of the bot, reconnect
                }
            }
            return Task.CompletedTask;
        }

        protected void SendPong()
        {
            string pong = buf.Replace("PING", "PONG");
            WriteLine(pong);
            TryLog(pong);
        }


        public override void SendMessage(IrcMessage message) 
            => WriteLine($"PRIVMSG {channelName} :{message.Content}");

        protected void Reconnect()
        {
            TryLog($"Disconnected. Re-establishing connection..");
            Dispose(true);

            while (!sock.Connected)
            {
                Setup();
                Start();
                Thread.Sleep(5000);
            }
        }

        internal override void Logout() => sock.Close();

        protected void Login()
        {
            WriteLine("PASS " + Token + "\r\nNICK " + nick + "\r\n");
            for (buf = input.ReadLine(); !((buf.Split(' ')[1] == "376" && this is OsuBot) || buf.Split(' ')[1] == "366"); buf = input.ReadLine())
            {
                if (buf == null || buf.Split(' ')[1] == "464" 
                    || (buf.StartsWith(":tmi.twitch.tv NOTICE * ") && (buf.EndsWith(":Improperly formatted auth") || buf.EndsWith(":Login authentication failed"))))
                {
                    throw new InvalidOrWrongTokenException(buf);
                }
                if (buf.Split(' ')[1] == "001")
                {
                    WriteLine($"JOIN {channelName}");
                    if(!testRun) TryLog($"Connected to {Name}-IRC.");
                }
            }
            if (this is TwitchBot t) t.GetMods();
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
