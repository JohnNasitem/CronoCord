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
using CronoCord.Utilities;

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
            await RespondWithModalAsync(new Interactions.Modals.CreateEventModal().Build());
        }


        [SlashCommand("view-events", "View all events")]
        public async Task ViewEvents()
        {
            List<Classes.Event> events = DatabaseManagement.GetEvents();
            string output = "";

            foreach (Classes.Event e in events)
            {
                output += e.Name;
            }

            await RespondAsync(output);
        }
    }
}
