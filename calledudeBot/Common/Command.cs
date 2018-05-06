using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace calledudeBot.Common
{
    class Command
    {
        private string response;
        private string cmdFile = calledudeBot.cmdFile;
        private MethodInfo specialMethod;
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
                    string[] args = { arg, response };
                    specialMethod.Invoke(commandHandler, args);
                    response = args[1];
                }
                return response;
            }
            set { response = value; }
        }
        public string Description { get; set; }
        public bool IsSpecial { get; }
        public bool UserAllowed { get; }
        public string Arguments
        {
            set { arg = value; }
        }
        public string[] AlternateName
        {
            get; set;
        }

        public Command(string cmd, string cmdToAdd, bool writeToFile, bool isSpecial = false)
        {
            if (cmd.Contains('<'))
            {
                int descriptionIndex = cmd.IndexOf('<');
                Description = cmd.Substring(descriptionIndex).Trim('<', '>');
                cmd = cmd.Remove(descriptionIndex);
            }
            if (!isSpecial)
            {
                int responseIndex = writeToFile ? cmd.IndexOf(cmd.Split(' ')[2]) : cmd.IndexOf(cmd.Split(' ')[1]);
                response = cmd.Substring(responseIndex).Trim();
            }
            else
            {
                IsSpecial = true;
                if (cmdToAdd == "!addcmd" || cmdToAdd == "!delcmd") UserAllowed = false;
                string cmdFunc = cmd.Trim();
                Type thisType = typeof(CommandHandler);
                specialMethod = thisType.GetMethod(cmdFunc, BindingFlags.NonPublic | BindingFlags.Instance);
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
