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
    public class EchoCommand : InteractionModuleBase<SocketInteractionContext>
    {
        public EchoCommand() { }

        [SlashCommand("ping", "Get bot latency")]
        public async Task Ping()
        => await RespondAsync($"Pong! Latency: {Context.Client.Latency} ms");

        [SlashCommand("info", "Get bot info")]
        public async Task Info()
        {
            await RespondAsync(embed: new EmbedBuilder()
                                    .WithTitle("Bot Information")
                                    .WithDescription("This is a bot that manages schedules as well a events")
                                    .WithColor(Color.Green)
                                    .AddField("Author", Program.Author, inline: true)
                                    .AddField("Version", Program.Version, inline: true)
                                    .Build());
        }
    }
}
