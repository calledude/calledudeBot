using System;
using System.Collections.Concurrent;
using System.Threading;

namespace calledudeBot.Services
{
    //Selection in console causes Console.WriteLine() to be permablocked (until selection is released), causing threads to get blocked permanently.
    //Therefore we run it on a dedicated logger thread.
    public static class Logger
    {
        private static readonly BlockingCollection<string> logQueue = new BlockingCollection<string>();

        static Logger()
        {
            log("[Logger] Started logging.");
            new Thread(run)
            {
                IsBackground = true
            }.Start();
        }

        public static void log(string logMessage) 
            => logQueue.Add(logMessage);

        private static void run()
        {
            while (true)
            {
                Console.WriteLine(logQueue.Take());
            }
        }
    }
}
