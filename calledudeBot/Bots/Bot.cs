using calledudeBot.Chat;

namespace calledudeBot.Bots
{
    public abstract class Bot
    {
        protected string token;
        public abstract void Start();
        public abstract void sendMessage(Message message);
    }
}
