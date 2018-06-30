using calledudeBot.Chat;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public abstract class Bot : IDisposable
    {
        protected string instanceName;
        protected string token;
        public abstract Task Start();
        public abstract void sendMessage(Message message);
        public abstract void Dispose();

        internal bool testRun = true;

        internal virtual async Task TryRun()
        {
            await Start();
            if (this is IrcClient irc)
            {
                irc.tryLogin();
            }
        }

        //Selection in console causes Console.WriteLine() to be permablocked, causing threads to get blocked permanently.
        //Therefore we run it on another thread.
        public virtual void tryLog(string message)
        {
            string msg = $"[{instanceName}]: {message}";
            Task.Run(async () =>
            {
                await Console.Out.WriteLineAsync(msg);
            });
        }
    }
}
