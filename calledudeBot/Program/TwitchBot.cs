using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;

namespace calledudeBot
{
    public class TwitchBot : IrcClient
    {
        private string cmdFile = calledudeBot.cmdFile;
        private MessageHandler messageHandler;
        public static List<string> mods = new List<string>();
        
        public override void Start(string token)
        {
            this.token = token;
            nick = "calledudeBot";
            port = 6667;
            server = "irc.chat.twitch.tv";
            channelName = "#calledude";
            botName = "Twitch";

            messageHandler = new MessageHandler(this);
            sock = new TcpClient();
            sock.Connect(server, port);
            output = new StreamWriter(sock.GetStream());
            input = new StreamReader(sock.GetStream());

            WriteLine("PASS " + token + "\r\n" +
                      "USER " + nick + " 0 * :" + nick + "\r\n" +
                      "NICK " + nick + "\r\n");
            WriteLine("CAP REQ :twitch.tv/commands");
            updateMods();

            Listen();
        }

        public override void Listen()
        {
            try
            {
                for (buf = input.ReadLine(); ; buf = input.ReadLine())
                {
                    //Console.WriteLine($"[{botName}]: {buf}");
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
                        Console.WriteLine($"[{botName}]: Connected to Twitch.");
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