using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Chat.Commands;
using calledudeBot.Chat.Info;
using calledudeBot.Services;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace calledudeBot.Config
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfig(this IServiceCollection services)
        {
            BotConfig config;
            if (File.Exists("config.json"))
            {
                var jsonString = File.ReadAllText("config.json");

                config = JsonConvert.DeserializeObject<BotConfig>(jsonString,
                    new JsonSerializerSettings()
                    {
                        Error = ParseErrorHandler
                    });
                return services.AddSingleton(config);
            }
            else
            {
                File.Create("config.json").Close();

                var cfg = JsonConvert.SerializeObject(new BotConfig(), Formatting.Indented);

                File.WriteAllText("config.json", cfg);

                Logger.Log("FATAL: No config file detected. Created one for you with default values, please fill it in.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
            => services
                .AddMediatR()
                .AddSingleton(_ =>
                    new DiscordSocketClient(new DiscordSocketConfig()
                    {
                        LogLevel = LogSeverity.Info
                    }))
                .AddSingleton<Hooky>()
                .AddSingleton<CommandHandler>()
                .AddSingleton<MessageDispatcher>()
                .AddSingleton<RelayHandler>()
                .AddScoped<TwitchMessageHandler>()
                .AddScoped<DiscordMessageHandler>()
                .AddScoped<OsuUserService>()
                .AddScoped<SongRequester>()
                .AddScoped<StreamMonitor>();

        public static IServiceCollection AddBots(this IServiceCollection services)
            => services
                .AddSingleton<DiscordBot>()
                .AddSingleton<OsuBot>()
                .AddSingleton<TwitchBot>();

        public static IServiceCollection AddCommands(this IServiceCollection services)
        {
            var commands = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.BaseType == typeof(SpecialCommand) || x.BaseType == typeof(SpecialCommand<CommandParameter>));

            foreach (var cmd in commands)
            {
                services.AddSingleton(typeof(Command), cmd);
            }
            return services;
        }

        private static void ParseErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs e)
        {
            e.ErrorContext.Handled = true;
        }
    }
}
