//***********************************************************************************
//Program: EventsModule.cs
//Description: Holds Events commands
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord.Interactions;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Reflection;

namespace CronoCord.Modules
{
    public class EventsModule : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventsModule"/>
        /// </summary>
        public EventsModule() { }


        [SlashCommand("create-event", "Create an event")]
        [DefaultMemberPermissionsAttribute(GuildPermission.ManageEvents)]
        public async Task CreateEvent()
        {
            await RespondWithModalAsync(new CreateEventModal().Build());
        }
    }

    public class CreateEventModal : ModalBuilder
    {
        public CreateEventModal()
        {
            Title = "Create Event";
            CustomId = "create_event";
            AddTextInput(label:"Event Name:",
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
            string startDate = components.First(x => x.CustomId == "event_start_date").Value;
            string endDate = components.First(x => x.CustomId == "event_end_date").Value;

            await modal.RespondAsync($"Name: {name}\nDescription: {desc}\nStart Date: {startDate}\nEnd Date: {endDate}");
        }
    }
}
