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
                UtilityMethods.PrettyConsoleWriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()} failed to execute in {cmdException.Context.Channel}.", UtilityMethods.LogLevel.Error);
                UtilityMethods.PrettyConsoleWriteLine(cmdException.ToString(), UtilityMethods.LogLevel.Error);
            }
            else if (message.Exception is InteractionException intException)
                UtilityMethods.PrettyConsoleWriteLine($"[Interaction/{message.Severity}] {message}", UtilityMethods.LogLevel.Info);
            else
                UtilityMethods.PrettyConsoleWriteLine($"[General/{message.Severity}] {message}", UtilityMethods.LogLevel.Info);

            return Task.CompletedTask;
        }
    }
}
