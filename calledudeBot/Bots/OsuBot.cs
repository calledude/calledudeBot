﻿using System;
using System.IO;
using System.Net.Sockets;

namespace calledudeBot.Bots
{
    public class OsuBot : IrcClient
    {

        public override void Start(string token)
        {
            this.token = token;
            channelName = nick = "calledude";
            server = "cho.ppy.sh";
            botName = "osu!";

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
                for (buf = input.ReadLine(); ; buf = input.ReadLine())
                {
                    if (buf.Split(' ')[1] == "001")
                    {
                        Console.WriteLine($"[{botName}]: Connected to osu!");
                    }

                    if (buf.StartsWith("PING "))
                    {
                        WriteLine(buf.Replace("PING", "PONG") + "\r\n");
                        Console.WriteLine($"[{botName}]: {buf.Replace("PING", "PONG")}");
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