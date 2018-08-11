using calledudeBot.Chat;
using calledudeBot.Services;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public abstract class Bot
    {
        protected string instanceName;
        protected string token;
        public abstract Task Start();
        public abstract void sendMessage(Message message);
        internal static bool testRun;
        protected APIHandler api;

        internal virtual async Task TryRun()
        {
            await Start();
            if (this is IrcClient irc)
            {
                irc.tryLogin();
            }
            Logout();
        }

        protected abstract void Logout();

        internal virtual void StartServices()
        {
            api?.Start();
            if (this is IrcClient irc && !testRun) irc.Listen();
        }

        //Selection in console causes Console.WriteLine() to be permablocked, causing threads to get blocked permanently.
        //Therefore we run it on another thread.
        //TODO: Create a dedicated thread for logging instead as to not clog up other threads with unfinished tasks.
        public virtual void tryLog(string message)
        {
            string msg = $"[{instanceName}]: {message}";
            Task.Run(async () =>
            {
                await Console.Out.WriteLineAsync(msg);
            });
        }
    }

    [Serializable]
    internal class InvalidOrWrongTokenException : Exception
    {
        public InvalidOrWrongTokenException(string message) : base(message)
        {
        }

    }
}
