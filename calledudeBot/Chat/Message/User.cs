using System;
using System.Threading.Tasks;

namespace calledudeBot.Chat
{
    public sealed class User
    {
        public string Name { get; }

        private readonly Func<Task<bool>> _isModFunc;

        public User(string userName, Func<Task<bool>> isModFunc)
        {
            Name = userName;
            _isModFunc = isModFunc;
        }

        public async Task<bool> IsModerator()
        {
            return await _isModFunc();
        }
    }
}
