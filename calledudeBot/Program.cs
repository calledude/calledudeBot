using calledudeBot.Bots;
using calledudeBot.Chat.Commands;
using calledudeBot.Config;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;
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
            services.ConfigureLogging();

            var logger = Log.Logger.ForContext(typeof(calledudeBot));

            const string cfgFile = "config.json";

            if (File.Exists(cfgFile))
            {
                var jsonString = File.ReadAllText(cfgFile);

                var config = JsonConvert.DeserializeObject<BotConfig>(jsonString,
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

                logger.Fatal("No config file detected. Created one for you with default values, please fill it in.");
                logger.Information("Press any key to exit..");
                Console.ReadKey();
                Environment.Exit(0);
            }

            services
                .AddBots()
                .AddCommands()
                .AddServices();

            var serviceProvider = services
                .BuildServiceProvider();

            var commands =
                JsonConvert.DeserializeObject<List<Command>>(File.ReadAllText(CommandUtils.CMDFILE))
                ?? new List<Command>();
            commands.AddRange(serviceProvider.GetServices<Command>());

            foreach (var cmd in commands)
            {
                CommandUtils.Commands.Add(cmd);
            }

            logger.Information($"Done. Loaded {CommandUtils.Commands.Count} commands.");

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
