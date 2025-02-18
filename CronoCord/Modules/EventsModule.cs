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
            await RespondAsync($"Creating event...");
        }
    }
}
