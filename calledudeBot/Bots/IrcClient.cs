using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
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
                StartServices();
                Thread.Sleep(5000);
            }
        }

        protected override void Logout()
        {
            sock.Close();
        }


        public virtual void tryLogin()
        {
            for (buf = input.ReadLine(); ; buf = input.ReadLine())
            {
                if (buf == null || buf.Split(' ')[1] == "464" || buf.StartsWith(":tmi.twitch.tv NOTICE * :Improperly formatted auth"))
                {
                    throw new InvalidOrWrongTokenException(buf);
                }
                else if (buf.Split(' ')[1] == "001")
                {
                    WriteLine($"JOIN {channelName}");
                }
                else if ((buf.Split(' ')[1] == "376" && this is OsuBot) || buf.Split(' ')[1] == "366")
                {
                    break;
                }
            }
        }

        protected virtual void WriteLine(string message)
        {
            output.WriteLine(message);
            output.Flush();
        }
    }
}
