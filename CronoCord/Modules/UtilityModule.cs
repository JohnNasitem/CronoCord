//***********************************************************************************
//Program: UtilityModule.cs
//Description: Holds ultility commands that arent for a specific feature
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace CronoCord.Modules
{
    public class UtilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UtilityModule"/>
        /// </summary>
        public UtilityModule() { }



        /// <summary>
        /// Ping slash command
        /// </summary>
        /// <returns>Task</returns>
        [SlashCommand("ping", "Get bot latency")]
        public async Task Ping() =>
            await RespondAsync($"Pong! Latency: {Context.Client.Latency} ms");



        /// <summary>
        /// Info slash command
        /// </summary>
        /// <returns>Task</returns>
        [SlashCommand("info", "Get bot info")]
        public async Task Info() =>
            await RespondAsync(embed: new EmbedBuilder()
                                    .WithTitle("Bot Information")
                                    .WithDescription("This is a bot that manages schedules as well a events")
                                    .WithColor(Color.Green)
                                    .AddField("Author", Program.Author, inline: true)
                                    .AddField("Version", Program.Version, inline: true)
                                    .Build());

    }
}
