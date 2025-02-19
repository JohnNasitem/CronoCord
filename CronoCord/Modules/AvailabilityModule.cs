//***********************************************************************************
//Program: AvailabilityModule.cs
//Description: Holds Availability commands
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
using CronoCord.Classes;
using System.Text.RegularExpressions;


namespace CronoCord.Modules
{
    public class AvailabilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AvailabilityModule"/>
        /// </summary>
        public AvailabilityModule() { }


        [SlashCommand("add-availability-slot", "Add an availability slot to your schedule")]
        public async Task AddAvailabilitySlotCommand()
        {
            await RespondWithModalAsync(new Interactions.Modals.CreateAvailabilityModal().Build());
        }


        [SlashCommand("view-schedule", "View schedules")]
        public async Task ViewScheduleCommand(
            [Summary("users", "The user whose schedule you want to view")] string usersStr = "",
            [Summary("week_offset", "The week offset for the schedule (positive only)")] int weekOffset = 0,
            [Summary("show_overlap-Count", "Whether to show the count of overlapping events")] bool showOverlapCount = false)
        {
            // List of users
            List<IUser> users = new List<IUser>();

            // Find all users from the usersStr
            foreach (Match match in Regex.Matches(usersStr, @"\d+"))
            {
                IUser possibleUser = Context.Client.GetUserAsync(ulong.Parse(match.Value)).Result;

                if (possibleUser != null)
                    users.Add(possibleUser);
            }

            // Default to invoking user
            if (usersStr == "")
                users.Add(Context.User);

            // Only positive week offsets
            if (weekOffset < 0)
                weekOffset = 0;

            await ViewSchedule(users, weekOffset, showOverlapCount);
        }

        private async Task ViewSchedule(List<IUser> users, int weekOffset, bool showOverlapCount)
        {
            List<Availability> unfiltered_availabilities = null;
            List<Availability> filtered_availabilities = null;

            //await Task.Run(() => unfiltered_availabilities = DatabaseManagement.GetAvailabilites());

            if (unfiltered_availabilities == null)
            {
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                return;
            }


            foreach (Availability availability in unfiltered_availabilities)
            {
                // Include the availabilities happning this week
                if (UtilityMethods.GetSundayUnixTimeStamp(false) <= availability.StartTimeUnix && availability.StartTimeUnix <= UtilityMethods.GetSundayUnixTimeStamp(true))
                    filtered_availabilities.Add(availability);
                // Including repeating availabilities
                //else if (availability.IsRecurring != Availability.Recurring.N)
                //{
                //    switch (availability.IsRecurring)
                //    {
                //        case Availability.Recurring.D:
                //            DateTime startDate = DateTimeOffset.FromUnixTimeSeconds(availability.StartTimeUnix).DateTime;


                //            break;
                //    }
                //}
            }

            await RespondAsync("temp");
        }

        private Image GenerateScheduleImage(List<Availability> availabilities, int weekOffset, bool showOverlapCount)
        {

        }
    }
}
