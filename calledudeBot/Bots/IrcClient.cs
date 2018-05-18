using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using calledudeBot.Chat;

namespace calledudeBot.Bots
{
    public abstract class IrcClient : Bot
    {
        protected TcpClient sock;
        protected TextWriter output;
        protected TextReader input;
        protected string nick;
        protected int port = 6667;
        protected string server;
        protected string buf;
        protected string channelName;
        protected string instanceName;
        protected string token;

        public abstract void Start(string token);
        public abstract void Listen();

        public override void sendMessage(Message message)
        {
            WriteLine($"PRIVMSG {channelName} :{message.Content}");
        }

        protected virtual void reconnect()
        {
            Console.WriteLine($"[{instanceName}]: Disconnected. Re-establishing connection..");
            sock.Dispose();
            while (!sock.Connected)
            {
                Start(token);
                Thread.Sleep(5000);
            }
        }

        protected virtual void WriteLine(string message)
        {
            output.WriteLine(message);
            output.Flush();
        }
    }
}
