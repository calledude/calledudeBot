using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Chat.Commands;
using calledudeBot.Config;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace calledudeBot
{
    public static class calledudeBot
    {
        private static void Main()
        {
            Console.Title = "calledudeBot";
            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
            {
                var isCtrlC = e.SpecialKey == ConsoleSpecialKey.ControlC;
                var isCtrlBreak = e.SpecialKey == ConsoleSpecialKey.ControlBreak;

                if (isCtrlC || isCtrlBreak) e.Cancel = true;
            };
            CleanCmdFile();

            var services = new ServiceCollection()
                .AddConfig()
                .AddBots()
                .AddCommands()
                .AddServices();

            var serviceProvider = services
                .BuildServiceProvider();

            serviceProvider
                .GetRequiredService<CommandHandler>()
                .Initialize();

            var bots = services
                .Where(x => x.ImplementationType?
                            .GetInterfaces()
                            .Any(y => y == typeof(IBot)) ?? false)
                .Select(x => serviceProvider.GetRequiredService(x.ServiceType))
                .Cast<IBot>();

            foreach(var bot in bots)
            {
                bot.Start();
            }

            serviceProvider.GetRequiredService<Hooky>().Start();
        }

        private static void CleanCmdFile()
        {
            if (!File.Exists(CommandUtils.CmdFile))
            {
                File.Create(CommandUtils.CmdFile).Close();
                return; //In this case, file is empty (newly created) -> no need for cleaning -> return
            }

            //Cleaning up
            List<string> cleanList = File.ReadAllLines(CommandUtils.CmdFile)
                                         .Where(p => !string.IsNullOrWhiteSpace(p))
                                         .Select(p => p.Trim())
                                         .ToList();

            File.WriteAllLines(CommandUtils.CmdFile, cleanList);
        }
    }
}
