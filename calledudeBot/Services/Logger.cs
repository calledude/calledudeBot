using Nito.AsyncEx;
using System;

namespace calledudeBot.Services
{
    public static class Logger
    {
        private static readonly AsyncCollection<string> _logQueue = new AsyncCollection<string>();

        static Logger()
        {
            Log("Started logging.");
            Run();
        }

        public static void Log(string logMessage, object sender = null)
            => _logQueue.AddAsync($"[{sender as string ?? sender?.GetType().Name ?? "Logger"}]: {logMessage}");

        private static async void Run()
        {
            while (true)
            {
                Console.WriteLine(await _logQueue.TakeAsync());
            }
        }
    }
}
