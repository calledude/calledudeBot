using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace calledudeBotREWRITE.Program
{
    class Command
    {
        private string cmdName;
        private string response;
        private string description;
        private string cmdFile = calledudeBot.cmdFile;
        private bool isSpecial;
        private bool userAllowed = true;
        private MethodInfo specialMethod;
        private CommandHandler commandHandler;
        private string arg;

        public string Name
        {
            get { return cmdName; }
        }

        public CommandHandler handlerInstance
        {
            set { commandHandler = value; }
        }

        public string Response
        {
            get
            {
                if(isSpecial)
                {
                    string[] args = { arg, response };
                    specialMethod.Invoke(commandHandler, args);
                    response = args[1];
                }
                return response;
            }
            set { response = value; }
        }
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        public bool IsSpecial
        {
            get { return isSpecial; }
        }
        public bool UserAllowed
        {
            get { return userAllowed; }
        }
        public string Arguments
        {
            set { arg = value; }
        }

        public Command(string cmd, string cmdToAdd, bool writeToFile, bool isSpecial = false)
        {
            if (cmd.Contains('<'))
            {
                int descriptionIndex = cmd.IndexOf('<');
                description = cmd.Substring(descriptionIndex).Trim('<', '>');
                cmd = cmd.Remove(descriptionIndex);
            }
            if (!isSpecial)
            {
                int responseIndex = writeToFile ? cmd.IndexOf(cmd.Split(' ')[2]) : cmd.IndexOf(cmd.Split(' ')[1]);
                response = cmd.Substring(responseIndex).Trim();
            }
            else
            {
                this.isSpecial = true;
                if (cmdToAdd == "!addcmd" || cmdToAdd == "!delcmd") userAllowed = false;
                string cmdFunc = cmd.Trim();
                Type thisType = typeof(CommandHandler);
                specialMethod = thisType.GetMethod(cmdFunc, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            cmdName = cmdToAdd;
            
            
            if (writeToFile)
            {
                string line = cmdName + " " + response + " <" + description + ">";
                File.AppendAllText(cmdFile, line + Environment.NewLine);
            }

        }
    }
}
