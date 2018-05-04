﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;
using calledudeBot.Common;
using calledudeBot.Services;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public class TwitchBot : IrcClient
    {
        private MessageHandler messageHandler;
        public static List<string> mods = new List<string>();
        private string osuUsername;
        
        public override void Start(string token)
        {
            string osuAPIToken = Common.calledudeBot.osuAPIToken;
            string osuNick = Common.calledudeBot.osuNick;
            string botNick = Common.calledudeBot.botNick;
            channelName = Common.calledudeBot.channelName;
            APIHandler api = new APIHandler($"https://osu.ppy.sh/api/get_user?k={osuAPIToken}&u={osuNick}", Caller.Twitch);
            api.DataReceived += checkUserUpdate;

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
            updateMods();

            Listen();
        }

        private Task checkUserUpdate(JsonData jsonData)
        {
            Console.WriteLine(jsonData.osuData[0].username);
            return Task.CompletedTask;
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
                        mods.Clear();
                        foreach (string s in modsArr)
                        {
                            mods.Add(s.Trim());
                        }
                    }
                    else if (buf.Split(' ')[1] == "001")
                    {
                        WriteLine($"JOIN {channelName}");
                        Console.WriteLine($"[{instanceName}]: Connected to Twitch.");
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

        public void updateMods()
        {
            WriteLine($"PRIVMSG {channelName} /mods");
        }


    }
}