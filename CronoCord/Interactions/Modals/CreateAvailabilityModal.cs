//***********************************************************************************
//Program: CreateAvailabilityModal.cs
//Description: Modal for creating an availability
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************


using CronoCord.Classes;
using CronoCord.Utilities;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronoCord.Interactions.Modals
{
    public class CreateAvailabilityModal : ModalBuilder
    {
        public CreateAvailabilityModal()
        {
            Title = "Create Availability Slot";
            CustomId = "create_availability";
            AddTextInput(label: "Date (MMM DD YYYY):",
                         customId: "availability_date",
                         style: TextInputStyle.Short,
                         required: true,
                         value: DateTime.Now.ToString("MMM d yyyy"));
            AddTextInput(label: "Start Time (12 hour format):",
                         customId: "availability_start_time",
                         style: TextInputStyle.Short,
                         required: true);
            AddTextInput(label: "End Time (12 hour format):",
                         customId: "availability_end_time",
                         style: TextInputStyle.Short,
                         required: true);
            AddTextInput(label: "Recurring (N, D, W, M, Y):",
                         customId: "availability_recurring",
                         style: TextInputStyle.Short,
                         required: true,
                         value: "N");
        }

        public static async Task ModelSubmit(SocketModal modal)
        {
            // Extract values from the modal's inputs
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            string dateStr = components.First(x => x.CustomId == "availability_date").Value;
            string startTimeStr = components.First(x => x.CustomId == "availability_start_time").Value;
            string endTimeStr = components.First(x => x.CustomId == "availability_end_time").Value;
            string recurringStr = components.First(x => x.CustomId == "availability_recurring").Value;
            DateTime? date = UtilityMethods.ParseDateTime(dateStr);
            DateTime? startTime = UtilityMethods.ParseDateTime(startTimeStr);
            DateTime? endTime = UtilityMethods.ParseDateTime(endTimeStr);

            string errorMessage = "";

            if (date == null)
                errorMessage += $"Date: \"{dateStr}\" is in the wrong format! Use MMM DD YYYY\n";
            if (startTime == null)
                errorMessage += $"Start time: \"{startTimeStr}\" is in the wrong format! Use 12 hour format\n";
            if (endTime == null)
                errorMessage += $"End time: \"{endTimeStr}\" is in the wrong format! Use 12 hour format\n";
            if (!Enum.TryParse(recurringStr, out Availability.Recurring recurring))
                errorMessage += $"Recurring value: {recurringStr} is not valid! Options: N (never), D (daily), W (weekly), M (monthly), Y (yearly)\n";

            if (errorMessage != "")
            {
                await modal.RespondAsync(errorMessage, ephemeral: true);
                return;
            }

            DateTime startDateTime = ((DateTime)date).Date.Add(((DateTime)startTime).TimeOfDay);
            DateTime endDateTime = ((DateTime)date).Date.Add(((DateTime)endTime).TimeOfDay);

            if (!(startDateTime < endDateTime))
            {
                await modal.RespondAsync("Start time must be before end time!", ephemeral: true);
                return;
            }


            bool success = false;

            Availability availabilityDetails = new Availability(modal.User.Id, new DateTimeOffset(startDateTime).ToUnixTimeSeconds(), new DateTimeOffset(endDateTime).ToUnixTimeSeconds(), recurring);
            await Task.Run(() => success = DatabaseManagement.CreateAvailability(availabilityDetails));

            if (success)
                await modal.RespondAsync(embed: availabilityDetails.CreateSucessEmbed(), ephemeral: true);
            else
                await modal.RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);

        }
    }
}
