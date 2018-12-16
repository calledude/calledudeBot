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

        public virtual void tryLog(string message)
        {
            string msg = $"[{instanceName}]: {message}";
            Logger.log(msg);
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
