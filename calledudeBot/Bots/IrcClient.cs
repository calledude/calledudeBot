using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using calledudeBot.Common;

namespace calledudeBot.Bots
{
    public abstract class IrcClient : Bot
    {
        public TcpClient sock;
        public TextWriter output;
        public TextReader input;
        public string nick;
        public string password;
        public int port = 6667;
        public string server;
        public string buf;
        public string channelName;
        public string botName;
        public string token;

        public override void sendMessage(Message message)
        {
            WriteLine($"PRIVMSG {channelName} :{message.Content}");
        }

        public abstract void Start(string token);
        public abstract void Listen();

        public virtual void reconnect()
        {
            Console.WriteLine($"[{botName}]: Disconnected. Re-establishing connection..");
            sock.Dispose();
            while (!sock.Connected)
            {
                Start(token);
                Thread.Sleep(5000);
            }
        }

        public virtual void WriteLine(string message)
        {
            output.WriteLine(message);
            output.Flush();
        }
    }
}
