using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
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

        public abstract void Listen();

        public override void sendMessage(Message message)
        {
            WriteLine($"PRIVMSG {channelName} :{message.Content}");
        }

        protected virtual void reconnect()
        {
            tryLog($"Disconnected. Re-establishing connection..");
            sock.Dispose();
            while (!sock.Connected)
            {
                Start();
                Thread.Sleep(5000);
            }
        }
        
        //Selection in console causes Console.WriteLine() to be permablocked, causing threads to get blocked permanently.
        //Therefore we run it on another thread.
        public virtual void tryLog(string message)
        {
            string msg = $"[{instanceName}]: {message}";
            Task.Run(() =>
            {
                Console.WriteLine(msg);
            });
        }
        protected virtual void WriteLine(string message)
        {
            output.WriteLine(message);
            output.Flush();
        }
    }
}
