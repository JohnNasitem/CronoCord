using Discord.WebSocket;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CronoCord.Utilities;
using System;

namespace CronoCord.Interactions.Modals
{
    public class CreateEventModal : ModalBuilder
    {
        public CreateEventModal()
        {
            Title = "Create Event";
            CustomId = "create_event";
            AddTextInput(label: "Event Name:",
                         customId: "event_name",
                         style: TextInputStyle.Short,
                         required: true);
            AddTextInput(label: "Event Description:",
                         customId: "event_desc",
                         style: TextInputStyle.Paragraph,
                         required: true);
            AddTextInput(label: "Start Date:",
                         customId: "event_start_date",
                         style: TextInputStyle.Short,
                         required: true);
            AddTextInput(label: "End Date:",
                         customId: "event_end_date",
                         style: TextInputStyle.Short,
                         required: true);
        }

        public static async Task ModelSubmit(SocketModal modal)
        {
            // Extract values from the modal's inputs
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            string name = components.First(x => x.CustomId == "event_name").Value;
            string desc = components.First(x => x.CustomId == "event_desc").Value;
            string startDateStr = components.First(x => x.CustomId == "event_start_date").Value;
            string endDateStr = components.First(x => x.CustomId == "event_end_date").Value;
            DateTime? startDateTime = UtilityMethods.ParseDateTime(startDateStr);
            DateTime? endDateTime = UtilityMethods.ParseDateTime(endDateStr);

            string errorMessage = "";

            if (startDateTime == null)
                errorMessage = $"Start date: \"{startDateStr}\" is in the wrong format!\n";
            if (endDateStr == null)
                errorMessage += $"End date: \"{endDateStr}\" is in the wrong format!";

            if (errorMessage != "")
            {
                await modal.RespondAsync(errorMessage, ephemeral: true);
                return;
            }

            DatabaseManagement.CreateEvent(new Classes.Event(modal.User.Id, name, desc, new DateTimeOffset((DateTime)startDateTime).ToUnixTimeSeconds(), new DateTimeOffset((DateTime)endDateTime).ToUnixTimeSeconds(), modal.Channel.Id));


            await modal.RespondAsync(embed: new EmbedBuilder()
                                    .WithTitle("Successfully created event!")
                                    .WithDescription($"Name: {name}" +
                                                     $"From {UtilityMethods.ToUnixTimeStamp((DateTime)startDateTime)} to {UtilityMethods.ToUnixTimeStamp((DateTime)endDateTime)}" +
                                                     $"Description: {desc}")
                                    .WithColor(Color.Green)
                                    .Build());
        }
    }
}
