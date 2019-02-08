using System;
using Nito.AsyncEx;

namespace calledudeBot.Services
{
    public static class Logger
    {
        private static readonly AsyncCollection<string> logQueue = new AsyncCollection<string>();

        static Logger()
        {
            Log("[Logger] Started logging.");
            run();
        }

        public static void Log(string logMessage) 
            => logQueue.AddAsync(logMessage);

        private static async void run()
        {
            while (true)
            {
                Console.WriteLine(await logQueue.TakeAsync());
            }
        }
    }
}
