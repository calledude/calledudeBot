﻿using calledudeBot.Chat;
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
        internal abstract void Logout();
        public abstract void sendMessage(Message message);
        protected abstract void Dispose(bool disposing);

        protected Bot(string name)
        {
            Name = name;
        }

        public void tryLog(string message)
        {
            string msg = $"[{Name}]: {message}";
            Logger.log(msg);
        }

        public void Dispose()
        {
            Dispose(true);
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
