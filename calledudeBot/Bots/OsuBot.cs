using System;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace calledudeBot.Bots
{
    public class OsuBot : IrcClient
    {
        public OsuBot(string token, string osuNick)
        {
            this.token = token;
            channelName = nick = osuNick;
            server = "cho.ppy.sh";
            instanceName = "osu!";
        }

        public override void Start()
        {
            sock = new TcpClient();
            sock.Connect(server, port);
            input = new StreamReader(sock.GetStream());
            output = new StreamWriter(sock.GetStream());

            WriteLine("PASS " + token + "\r\n" +
                "USER " + nick + " 0 * :" + nick + "\r\n" +
                "NICK " + nick + "\r\n");
            Listen();
        }

        public override void Listen()
        {
            try
            {
                for (buf = input.ReadLine();; buf = input.ReadLine())
                {
                    if (buf.StartsWith("PING "))
                    {
                        string pong = buf.Replace("PING", "PONG");
                        WriteLine(pong);
                        tryLog(pong);
                    }
                    else if (buf.Split(' ')[1] == "001")
                    {
                        tryLog($"Connected to osu!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                reconnect();
            }
        }
    }
}