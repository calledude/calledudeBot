﻿using calledudeBot.Chat.Info;

namespace calledudeBot.Chat.Commands
{
    public abstract class SpecialCommand<T> : Command where T : CommandParameter
    {
        protected abstract string specialFunc(T param);
        public virtual string GetResponse(T param) => specialFunc(param);
    }

    public abstract class SpecialCommand : Command
    {
        protected abstract string specialFunc();
        public virtual string GetResponse() => specialFunc();
    }
}
