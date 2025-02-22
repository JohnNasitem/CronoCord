//***********************************************************************************
//Program: UtilityModule.cs
//Description: Holds ultility commands that arent for a specific feature
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Discord;
using Discord.Interactions;

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
        public async Task PingCommand() =>
            await RespondAsync($"Pong! Latency: {Context.Client.Latency} ms");



        /// <summary>
        /// Info slash command
        /// </summary>
        /// <returns>Task</returns>
        [SlashCommand("info", "Get bot info")]
        public async Task InfoCommand() =>
            await RespondAsync(embed: new EmbedBuilder()
                                    .WithTitle("Bot Information")
                                    .WithDescription("This is a bot that manages schedules")
                                    .WithColor(Color.DarkerGrey)
                                    .AddField("Author", Program.Author, inline: true)
                                    .AddField("Version", Program.Version, inline: true)
                                    .Build());



        /// <summary>
        /// Help slash command, gives description for each command
        /// </summary>
        /// <returns>Task</returns>
        [SlashCommand("help", "Get information about a command")]
        public async Task HelpCommand(SlashCommands command)
        {
            EmbedBuilder embed = new EmbedBuilder().WithTitle(Enum.GetName(typeof(SlashCommands), command)).WithColor(Color.Blue);

            switch (command)
            {
                case SlashCommands.Ping:
                    embed.WithDescription("Checks the latency of the bot.");
                    break;
                case SlashCommands.Info:
                    embed.WithDescription("Provides information about the bot.");
                    break;
                case SlashCommands.Help:
                    embed.WithDescription("Describes the specified command in more detail.");
                    break;
                case SlashCommands.AddAvailabilitySlot:
                    embed.WithDescription("Adds an available time slot to the schedule.\n**Modal Inputs:**")
                         .AddField("Date", "Date the availability slot is happening in. Format in either MMM DD YYYY or MM/DD/YYYY, eg Feb 21 2025 or 2/21/2025", false)
                         .AddField("Start Time", "When the availabilty starts. Must be before End Time. eg 3pm, 4:10 am, 5:49pm", false)
                         .AddField("End Time", "When the availability ends. Must be after Start Time. eg 3pm, 4:10 am, 5:49pm", false)
                         .AddField("Recurring", "Does the availability repeat. All that matters is the first letter. (N)ever, (D)aily, (W)eekly, (M)onthly, (Y)early", false);
                    break;
                case SlashCommands.ViewSchedule:
                    embed.WithDescription("Displays the current schedule.\n**Arguments:**")
                         .AddField("users", "Users you want to get schedules for. Must be a user mention or user id, @CronoCord or 1341475663022133340. Defaults to invoking user.", false)
                         .AddField("week_offset", "How many weeks ahead you want to view. Must be a positive number, 1 means next week, 2 means week after next week, etc. Defaults to 0.", false)
                         .AddField("show_overlap_count", "Should the schedule slot the number of overlaps in each cell. Defaults to false", false);
                    break;
                case SlashCommands.EditSchedule:
                    embed.WithDescription("Edits an existing time slot in the schedule.\n**Modal Inputs:**")
                         .AddField("Date", "Date the availability slot is happening in. Format in either MMM DD YYYY or MM/DD/YYYY, eg Feb 21 2025 or 2/21/2025", false)
                         .AddField("Start Time", "When the availabilty starts. Must be before End Time. eg 3pm, 4:10 am, 5:49pm", false)
                         .AddField("End Time", "When the availability ends. Must be after Start Time. eg 3pm, 4:10 am, 5:49pm", false)
                         .AddField("Recurring", "Does the availability repeat. All that matters is the first letter. (N)ever, (D)aily, (W)eekly, (M)onthly, (Y)early", false)
                         .AddField("Delete Slot", "Should the slot be deleted. If the input for this field starts with a \"Y\" then it will delete the availability", false);
                    break;
                default:
                    embed.WithDescription("Command doesnt have a description yet. Tell <@{Program.AuthorID}> to make one.");
                    break;
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }


        public enum SlashCommands
        {
            // Utility
            Ping,
            Info,
            Help,

            // Availability
            AddAvailabilitySlot,
            ViewSchedule,
            EditSchedule
        }
    }
}
