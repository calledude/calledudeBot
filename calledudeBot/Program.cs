using calledudeBot.Bots;
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

            CommandUtils.Commands =
                JsonConvert.DeserializeObject<List<Command>>(File.ReadAllText(CommandUtils.CmdFile))
                ?? new List<Command>();
            CommandUtils.Commands.AddRange(serviceProvider.GetServices<Command>());
            Logger.Log($"Done. Loaded {CommandUtils.Commands.Count} commands.");

            var bots = services
                .Where(x => x.ImplementationType?
                            .GetInterfaces()
                            .Any(y => y == typeof(IBot)) ?? false)
                .Select(x => serviceProvider.GetRequiredService(x.ServiceType))
                .Cast<IBot>();

            foreach (var bot in bots)
            {
                bot.Start();
            }

            serviceProvider.GetRequiredService<Hooky>().Start();
        }
    }
}
