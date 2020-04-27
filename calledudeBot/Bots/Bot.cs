using calledudeBot.Chat;
using calledudeBot.Services;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public interface IBot : IDisposable
    {
        Task Start();
        Task Logout();
        void Log(string message);
    }

    public abstract class Bot<T> : IBot where T : Message<T>
    {
        protected Bot()
        {
            Name = GetType().Name.Replace("Bot", "");
        }

        public string Name { get; protected set; }
        protected abstract string Token { get; }

        public abstract Task Start();
        public abstract Task Logout();
        protected abstract Task SendMessage(T message);
        public abstract void Dispose(bool disposing);

        public void Dispose()
            => Dispose(true);

        public async Task SendMessageAsync(T message)
        {
            Log($"Sending message: {message.Content}");
            await SendMessage(message);
        }

        public void Log(string message)
        {
            Logger.Log(message, Name);
        }
    }

    [Serializable]
    internal class InvalidOrWrongTokenException : Exception
    {
    }
}
