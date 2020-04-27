using calledudeBot.Bots;
using MediatR;

namespace calledudeBot.Models
{
    public class ReadyNotification : INotification
    {
        public IBot Bot { get; }

        public ReadyNotification(IBot bot)
        {
            Bot = bot;
        }
    }
}
