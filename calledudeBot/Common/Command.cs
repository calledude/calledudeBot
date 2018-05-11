using System;
using System.IO;
using System.Linq;

namespace calledudeBot.Common
{
    class Command
    {
        public delegate void Action<T1, T2>(T1 obj, out T2 obj2);
        private Action<string, string> specialFunc;
        private string response;
        private string cmdFile = calledudeBot.cmdFile;
        private CommandHandler commandHandler;
        private string arg;
        public string Name { get; }

        public CommandHandler handlerInstance
        {
            set { commandHandler = value; }
        }
        public string Response
        {
            get
            {
                if(IsSpecial)
                {
                    specialFunc.Invoke(arg, out response);
                }
                return response;
            }
            set { response = value; }
        }
        public string Description { get; set; }
        public bool IsSpecial { get; }
        public bool UserAllowed { get; set; }
        public string Arguments
        {
            set { arg = value; }
        }
        public string[] AlternateName
        {
            get; set;
        }

        public Command(string cmd, string cmdToAdd, Action<string, string> specialFunc = null, bool writeToFile = false)
        {
            if (cmd.Contains('<'))
            {
                int descriptionIndex = cmd.IndexOf('<');
                Description = cmd.Substring(descriptionIndex).Trim('<', '>');
                cmd = cmd.Remove(descriptionIndex);
            }
            if (specialFunc == null)
            {
                int responseIndex = writeToFile ? cmd.IndexOf(cmd.Split(' ')[2]) : cmd.IndexOf(cmd.Split(' ')[1]);
                response = cmd.Substring(responseIndex).Trim();
            }
            else
            {
                IsSpecial = true;
                this.specialFunc = specialFunc;
            }
            Name = cmdToAdd;
            
            if (writeToFile)
            {
                string line = Name + " " + Response + " <" + Description + ">";
                File.AppendAllText(cmdFile, line + Environment.NewLine);
            }

        }
    }
}
