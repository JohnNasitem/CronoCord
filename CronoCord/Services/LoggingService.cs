//***********************************************************************************
//Program: LoggingService.cs
//Description: Logging service from the discord.net docs 
//Src: https://docs.discordnet.dev/guides/concepts/logging.html#usage-in-commands
//Date: Feb 17, 2025
//Author: John Nasitem
//***********************************************************************************

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

namespace CronoCord.Services
{
    public class LoggingService
    {
        public LoggingService(DiscordSocketClient client, CommandService command, InteractionService handler)
        {
            client.Log += LogAsync;
            command.Log += LogAsync;
            handler.Log += LogAsync;
        }
        private Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else if (message.Exception is InteractionException intException)
            {
                Console.WriteLine($"[Interaction/{message.Severity}] {message}");
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}
