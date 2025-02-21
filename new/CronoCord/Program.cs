//***********************************************************************************
//Program: Program.cs
//Description: Main file
//Date: Feb 17, 2025
//Author: John Nasitem
//***********************************************************************************



using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using CronoCord.Services;
using Discord.Commands;
using Discord.Interactions;

namespace CronoCord
{
    internal class Program
    {
        private static DiscordSocketClient? _client;
        private static IServiceProvider? _serviceProvider;
        public static string Version { get; } = "1.0.0";
        public static string Author { get; } = "Eatdatpizza";
        public static ulong AuthorID { get; } = 357657793215332357;

        static IServiceProvider CreateProvider()
        {
            // Config used by DiscordSocketClient
            // Define intents for the client
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers,
                AlwaysDownloadUsers = true,
            };

            ServiceCollection collection = new ServiceCollection();
            collection.AddSingleton(config);
            collection.AddSingleton<DiscordSocketClient>(); // Depends on config
            collection.AddSingleton<CommandService>();
            collection.AddSingleton<LoggingService>();      // Depends on DiscordSocketClient and CommandService
            collection.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));
            collection.AddScoped<InteractionHandler>();     // Depends on DiscordSocketClient, InteractionService, and IServiceProvider


            // Build and return the service provider
            return collection.BuildServiceProvider();
        }

        public static async Task Main(string[] args)
        {
            while (true)
            {
                try
                {
                    _serviceProvider = CreateProvider();

                    _client = _serviceProvider.GetService<DiscordSocketClient>();

                    // Here we can initialize the service that will register and execute our commands
                    await _serviceProvider.GetRequiredService<InteractionHandler>()
                        .InitializeAsync();

                    // Log in witt bot token
                    // Token is stored in (on windows) System Properties -> Environment Variables -> User variables
                    // Make sure to restart visual studio after adding a new user variable
                    await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("CRONOCORD_BOT_TOKEN"));

                    // Start bot and block the program until it is closed.
                    await _client.StartAsync();
                    await Task.Delay(Timeout.Infinite);
                }
                catch (Exception ex)
                {
                    UtilityMethods.PrettyConsoleWriteLine($"Bot crashed: {ex}", UtilityMethods.LogLevel.Critical);
                    await Task.Delay(5000);
                    UtilityMethods.PrettyConsoleWriteLine($"Attempting to restart", UtilityMethods.LogLevel.Info);
                }
            }
            
        }
    }
}
