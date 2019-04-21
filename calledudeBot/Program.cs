using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Chat.Commands;
using calledudeBot.Config;
using calledudeBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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

            CleanCmdFile();

            var services = new ServiceCollection();
            const string cfgFile = "config.json";
            BotConfig config;

            if (File.Exists(cfgFile))
            {
                var jsonString = File.ReadAllText(cfgFile);

                config = JsonConvert.DeserializeObject<BotConfig>(jsonString,
                    new JsonSerializerSettings()
                    {
                        Error = (s, e) => e.ErrorContext.Handled = true
                    });
                services.AddSingleton(config);
            }
            else
            {
                File.Create(cfgFile).Close();

                var cfg = JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented);

                File.WriteAllText(cfgFile, cfg);

                Logger.Log("FATAL: No config file detected. Created one for you with default values, please fill it in.");
                Logger.Log("Press any key to exit..");
                Console.ReadKey();
                Environment.Exit(0);
            }

            services
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
