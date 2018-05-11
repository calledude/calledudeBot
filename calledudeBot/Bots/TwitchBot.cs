using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using calledudeBot.Chat;
using calledudeBot.Services;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;

namespace calledudeBot.Bots
{
    public class TwitchBot : IrcClient
    {
        private MessageHandler messageHandler;
        public List<string> mods = new List<string>();
        private Timer timer;
        private bool modCheckLock = false;

        public override void Start(string token)
        {
            string osuAPIToken = calledudeBot.osuAPIToken;
            string osuNick = calledudeBot.osuNick;
            string botNick = calledudeBot.botNick;
            channelName = calledudeBot.channelName;
            APIHandler api = new APIHandler($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}", Caller.Twitch);
            api.DataReceived += checkUserUpdate;
            api.Start();

            this.token = token;
            server = "irc.chat.twitch.tv";
            instanceName = "Twitch"; 

            messageHandler = new MessageHandler(this);
            sock = new TcpClient();
            sock.Connect(server, port);
            output = new StreamWriter(sock.GetStream());
            input = new StreamReader(sock.GetStream());

            WriteLine("PASS " + token + "\r\n" +
                      "USER " + botNick + " 0 * :" + botNick + "\r\n" +
                      "NICK " + botNick + "\r\n");
            WriteLine("CAP REQ :twitch.tv/commands");

            timer = new Timer(30000);
            timer.Elapsed += modLockEvent;
            timer.Start();

            Listen();
        }

        private void checkUserUpdate(JsonData jsonData)
        {
            Console.WriteLine(jsonData.osuData[0].username);
        }

        public override void Listen()
        {
            try
            {
                for (buf = input.ReadLine(); ; buf = input.ReadLine())
                {
                    if (buf.StartsWith("PING "))
                    {
                        WriteLine(buf.Replace("PING", "PONG") + "\r\n");
                        Console.WriteLine($"[Twitch]: {buf.Replace("PING", "PONG")}");
                    }

                    if (buf[0] != ':') continue;

                    else if (buf.StartsWith($":tmi.twitch.tv NOTICE {channelName} :The moderators of this channel are:"))
                    {
                        int modsIndex = buf.LastIndexOf(':') + 1;
                        var modsArr = buf.Substring(modsIndex).Split(',');
                        mods = modsArr.Select(x => x.Trim()).ToList();
                    }
                    else if (buf.Split(' ')[1] == "001")
                    {
                        WriteLine($"JOIN {channelName}");
                        Console.WriteLine($"[{instanceName}]: Connected to Twitch.");
                    }
                    else if(buf.Split(' ')[1] == "366")
                    {
                        getMods();
                    }
                    else if (buf.Split(' ')[1] == "PRIVMSG") //this is something else, check for message
                    {
                        Message message = new Message(buf, this);
                        messageHandler.determineResponse(message);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                reconnect(); //Since basically any exception will break the fuck out of the bot, reconnect
            }
        }

        private void modLockEvent(object sender, ElapsedEventArgs e)
        {
            modCheckLock = false;
            timer.Stop();
        }

        public List<string> getMods()
        {
            if(!modCheckLock) WriteLine($"PRIVMSG {channelName} /mods");
            return mods;
        }


    }
}