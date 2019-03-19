using Nito.AsyncEx;
using System;

namespace calledudeBot.Services
{
    public static class Logger
    {
        private static readonly AsyncCollection<string> _logQueue = new AsyncCollection<string>();

        static Logger()
        {
            Log("[Logger] Started logging.");
            run();
        }

        public static void Log(string logMessage)
            => _logQueue.AddAsync(logMessage);

        private static async void run()
        {
            while (true)
            {
                Console.WriteLine(await _logQueue.TakeAsync());
            }
        }
    }
}
