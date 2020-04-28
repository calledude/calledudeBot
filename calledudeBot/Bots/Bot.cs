using calledudeBot.Chat;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public interface IBot : IDisposable
    {
        Task Start();
        Task Logout();
    }

    public abstract class Bot<T> : IBot where T : Message<T>
    {
        private readonly ILogger _logger;

        protected Bot(ILogger logger)
        {
            _logger = logger;
        }

        protected abstract string Token { get; }

        public abstract Task Start();
        public abstract Task Logout();
        protected abstract Task SendMessage(T message);
        public abstract void Dispose(bool disposing);

        public void Dispose()
            => Dispose(true);

        public async Task SendMessageAsync(T message)
        {
            _logger.LogInformation("Sending message: {0}", message.Content);
            await SendMessage(message);
        }
    }

    [Serializable]
    internal class InvalidOrWrongTokenException : Exception
    {
    }
}
