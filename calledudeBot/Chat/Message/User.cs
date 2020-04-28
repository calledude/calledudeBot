using Nito.AsyncEx;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public sealed class User
    {
        public string Name { get; }

        private readonly AsyncLazy<bool> _isModerator;

        public User(string userName, Func<Task<bool>> isModFunc)
        {
            Name = userName;
            _isModerator = new AsyncLazy<bool>(isModFunc);
        }

        public async Task<bool> IsModerator()
            => await _isModerator;
    }
}
