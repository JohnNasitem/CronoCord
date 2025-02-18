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
using Discord.WebSocket;

namespace CronoCord.Services
{
    /// <summary>
    /// Logging service
    /// </summary>
    public class LoggingService
    {
        public LoggingService(DiscordSocketClient client, CommandService command)
        {
            client.Log += LogAsync;
            command.Log += LogAsync;
        }
        private Task LogAsync(LogMessage message)
        {
            if (message.Exception is CommandException cmdException)
            {
                Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                    + $" failed to execute in {cmdException.Context.Channel}.");
                Console.WriteLine(cmdException);
            }
            else
                Console.WriteLine($"[General/{message.Severity}] {message}");

            return Task.CompletedTask;
        }
    }
}
