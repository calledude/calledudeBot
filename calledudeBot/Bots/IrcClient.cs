using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using calledudeBot.Chat;
using calledudeBot.Services;

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


        protected IrcClient(string server)
        {
            this.server = server;
            Setup();
        }

        protected void Setup()
        {
            sock = new TcpClient();
            sock.Connect(server, port);
            output = new StreamWriter(sock.GetStream());
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
                    Logger.log(e.Message);
                    reconnect(); //Since basically any exception will break the fuck out of the bot, reconnect
                }
            }
            return Task.CompletedTask;
        }

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
                Setup();
                Start();
                Thread.Sleep(5000);
            }
        }

        internal override void Logout()
        {
            sock.Close();
        }


        public virtual void Login()
        {
            WriteLine("PASS " + token + "\r\n" + "NICK " + nick + "\r\n");
            for (buf = input.ReadLine(); ; buf = input.ReadLine())
            {
                if (buf == null || buf.Split(' ')[1] == "464" 
                    || buf.StartsWith(":tmi.twitch.tv NOTICE * ") && (buf.EndsWith(":Improperly formatted auth") || buf.EndsWith(":Login authentication failed")))
                {
                    throw new InvalidOrWrongTokenException(buf);
                }
                if (buf.Split(' ')[1] == "001")
                {
                    WriteLine($"JOIN {channelName}");
                    if(!testRun) tryLog($"Connected to {instanceName}-IRC.");
                }
                else if ((buf.Split(' ')[1] == "376" && this is OsuBot) || buf.Split(' ')[1] == "366") //Signifies a successful login
                {
                    if(this is TwitchBot t) t.getMods();
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
