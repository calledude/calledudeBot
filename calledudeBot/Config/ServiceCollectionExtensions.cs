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
    }
}
