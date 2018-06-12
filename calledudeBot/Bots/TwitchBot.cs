using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using calledudeBot.Chat;
using calledudeBot.Services;
using System.Timers;
using System.Linq;

namespace calledudeBot.Bots
{
    public class TwitchBot : IrcClient
    {
        private MessageHandler messageHandler;
        private List<string> mods = new List<string>();
        private Timer modLockTimer;
        private bool modCheckLock;
        private OsuUserData oldOsuData;
        public override void Start(string token)
        {
            string osuAPIToken = calledudeBot.osuAPIToken;
            string osuNick = calledudeBot.osuNick;
            string botNick = calledudeBot.botNick;
            channelName = calledudeBot.channelName;
            APIHandler api = new APIHandler($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}", RequestData.OsuUser);
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

            modLockTimer = new Timer(60000);
            modLockTimer.Elapsed += modLockEvent;
            modLockTimer.Start();

            Listen();
        }

        private void checkUserUpdate(JsonData jsonData)
        {
            OsuUserData newOsuData = jsonData?.osuUserData[0];
            if (oldOsuData != null && newOsuData != null)
            {
                if(oldOsuData.pp_rank != newOsuData.pp_rank && Math.Abs(newOsuData.pp_raw - oldOsuData.pp_raw) >= 0.1)
                {
                    int rankDiff = newOsuData.pp_rank - oldOsuData.pp_rank;
                    float ppDiff = newOsuData.pp_raw - oldOsuData.pp_raw;

                    string formatted = string.Format("{0:0.00}", ppDiff < 0 ? -ppDiff : ppDiff);
                    string rankMessage = $"{Math.Abs(rankDiff)} ranks (#{newOsuData.pp_rank}). ";
                    string ppMessage = $"PP: {formatted}pp ({newOsuData.pp_raw}pp)";

                    sendMessage(new Message(newOsuData.username + " just" + (rankDiff < 0 ? " gained " : " lost ") + rankMessage + (ppDiff < 0 ? "+" : "-") + ppMessage));
                }
            }
            oldOsuData = newOsuData;
        }

        public override void Listen()
        {
            try
            {
                for (buf = input.ReadLine(); ; buf = input.ReadLine())
                {
                    var b = buf.Split(' ');
                    if (b[1] == "PRIVMSG") //This is a private message, check if we should respond to it.
                    {
                        Message message = new Message(buf, this);
                        messageHandler.determineResponse(message);
                    }
                    else if (buf.StartsWith($":tmi.twitch.tv NOTICE {channelName} :The moderators of this channel are:"))
                    {
                        int modsIndex = buf.LastIndexOf(':') + 1;
                        var modsArr = buf.Substring(modsIndex).Split(',');
                        mods = modsArr.Select(x => x.Trim()).ToList();
                    }
                    else if (b[0] == "PING")
                    {
                        WriteLine(buf.Replace("PING", "PONG") + "\r\n");
                        Console.WriteLine($"[Twitch]: {buf.Replace("PING", "PONG")}");
                    }
                    else if (b[1] == "001")
                    {
                        WriteLine($"JOIN {channelName}");
                        Console.WriteLine($"[{instanceName}]: Connected to Twitch.");
                    }
                    else if (b[1] == "366")
                    {
                        getMods();
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
            modLockTimer.Stop(); 
        }

        public List<string> getMods()
        {
            if(!modCheckLock) WriteLine($"PRIVMSG {channelName} /mods");
            modCheckLock = true;
            modLockTimer.Start();
            return mods;
        }


    }
}