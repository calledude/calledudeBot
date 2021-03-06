﻿using calledudeBot.Bots;
using calledudeBot.Chat;
using calledudeBot.Chat.Commands;
using calledudeBot.Services;
using Discord;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Linq;
using System.Reflection;

namespace calledudeBot.Config
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureLogging(this IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(LogEventLevel.Information, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext:l}] {Message:lj}{NewLine}{Exception}")
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .CreateLogger();

            return services.AddLogging(x => x.AddSerilog());
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
            => services
                .AddMediatR(Assembly.GetExecutingAssembly())
                .AddHttpClient()
                .AddSingleton(_ =>
                    new DiscordSocketClient(new DiscordSocketConfig()
                    {
                        LogLevel = LogSeverity.Info
                    }))
                .AddSingleton<Hooky>()
                .AddScoped<TwitchCommandHandler>()
                .AddScoped<DiscordCommandHandler>()
                .AddSingleton<MessageDispatcher>()
                .AddSingleton<RelayHandler>()
                .AddScoped(typeof(APIHandler<>))
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
                .Where(x => x.IsSubclassOf(typeof(Command)) && !x.IsAbstract);

            foreach (var cmd in commands)
            {
                services.AddSingleton(typeof(Command), cmd);
            }

            return services;
        }
    }
}
