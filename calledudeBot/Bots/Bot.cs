using calledudeBot.Chat;
using calledudeBot.Services;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public abstract class Bot : IDisposable
    {
        internal static bool testRun;

        protected string Name { get; }
        protected string Token { get; set; }

        internal abstract Task Start();
        internal abstract Task Logout();
        protected abstract void Dispose(bool disposing);

        protected Bot(string name) => Name = name;

        public void TryLog(string message)
        {
            string msg = $"[{Name}]: {message}";
            Logger.Log(msg);
        }

        public void Dispose() => Dispose(true);
    }

    public abstract class Bot<T> : Bot where T : Message
    {
        protected Bot(string name) : base(name)
        {
        }

        public abstract void SendMessage(T message);
    }

    [Serializable]
    internal class InvalidOrWrongTokenException : Exception
    {
        public InvalidOrWrongTokenException(string message) : base(message)
        {
        }
    }
}
