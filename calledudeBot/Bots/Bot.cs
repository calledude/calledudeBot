﻿using calledudeBot.Chat;
using calledudeBot.Services;
using System;
using System.Threading.Tasks;

namespace calledudeBot.Bots
{
    public abstract class Bot : IDisposable
    {
        internal static bool TestRun { get; set; }

        public string Name { get; }
        protected string Token { get; }

        internal abstract Task Start();
        internal abstract Task Logout();
        protected abstract void Dispose(bool disposing);

        protected Bot(string name, string token)
        {
            Name = name;
            Token = token;
        }

        public void TryLog(string message)
        {
            string msg = $"[{Name}]: {message}";
            Logger.Log(msg);
        }

        public void Dispose() => Dispose(true);
    }

    public abstract class Bot<T> : Bot where T : Message
    {
        protected Bot(string name, string token) : base(name, token)
        {
        }

        public async Task SendMessageAsync(T message)
        {
            TryLog($"Sending message: {message.Content}");
            await SendMessage(message);
        }

        protected abstract Task SendMessage(T message);
    }

    [Serializable]
    internal class InvalidOrWrongTokenException : Exception
    {
    }
}
