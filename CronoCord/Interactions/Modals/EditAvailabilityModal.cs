//***********************************************************************************
//Program: EditAvailabilityModal.cs
//Description: Modal for editting an availability
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
using Discord.Interactions;

namespace CronoCord.Interactions.Modals
{
    public class EditAvailabilityModal : ModalBuilder
    {
        public EditAvailabilityModal(Availability availabilityToEdit)
        {
            DateTime startTime = UtilityMethods.ToDateTime(availabilityToEdit.StartTimeUnix);
            DateTime endTime = UtilityMethods.ToDateTime(availabilityToEdit.EndTimeUnix);

            Title = "Edit Availability Slot";
            CustomId = $"edit_availability:{availabilityToEdit.StartTimeUnix},{availabilityToEdit.EndTimeUnix},{Enum.GetName(typeof(Availability.Recurring), availabilityToEdit.IsRecurring)}";
            AddTextInput(label: "Date (MMM DD YYYY):",
                         customId: "availability_date",
                         style: TextInputStyle.Short,
                         required: true,
                         value: startTime.ToString("MMM d yyyy"));
            AddTextInput(label: "Start Time (12 hour format):",
                         customId: "availability_start_time",
                         style: TextInputStyle.Short,
                         required: true,
                         value: startTime.ToString("hh:mm tt"));
            AddTextInput(label: "End Time (12 hour format):",
                         customId: "availability_end_time",
                         style: TextInputStyle.Short,
                         required: true,
                         value: endTime.ToString("hh:mm tt"));
            AddTextInput(label: "Recurring (N, D, W, M, Y):",
                         customId: "availability_recurring",
                         style: TextInputStyle.Short,
                         required: true,
                         value: Enum.GetName(typeof(Availability.Recurring), availabilityToEdit.IsRecurring));
            AddTextInput(label: "Delete Slot? (Y/N):",
                         customId: "availability_delete",
                         style: TextInputStyle.Short,
                         required: true,
                         value: "N");
        }

        public static async Task ModelSubmit(SocketModal modal)
        {
            // Extract old availability data from the modal's custom id
            string[] oldAvailabilityData = modal.Data.CustomId.Split(':')[1].Split(',');
            Availability oldAvailability = new Availability(modal.User.Id, long.Parse(oldAvailabilityData[0]), long.Parse(oldAvailabilityData[1]), (Availability.Recurring)Enum.Parse(typeof(Availability.Recurring), oldAvailabilityData[2]));

            // Extract values from the modal's inputs
            List<SocketMessageComponentData> components = modal.Data.Components.ToList();
            string dateStr = components.First(x => x.CustomId == "availability_date").Value;
            string startTimeStr = components.First(x => x.CustomId == "availability_start_time").Value;
            string endTimeStr = components.First(x => x.CustomId == "availability_end_time").Value;
            string recurringStr = components.First(x => x.CustomId == "availability_recurring").Value;
            string deleteStr = components.First(x => x.CustomId == "availability_delete").Value.ToUpper();
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
            if (!(deleteStr == "N" || deleteStr == "Y"))
                errorMessage += $"Delete Slot value: {deleteStr} is not valid! Options: Y (yes), N (no)";

            if (errorMessage != "")
            {
                await modal.RespondAsync(errorMessage, ephemeral: true);
                return;
            }

            bool success = false;

            if (deleteStr == "Y")
            {
                await Task.Run(() => success = DatabaseManagement.DeleteAvailability(oldAvailability));

                if (success)
                    await modal.RespondAsync($"Successfully deleted the availability slot!", ephemeral: true);
                else
                    await modal.RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);

                return;
            }

            DateTime startDateTime = ((DateTime)date).Date.Add(((DateTime)startTime).TimeOfDay);
            DateTime endDateTime = ((DateTime)date).Date.Add(((DateTime)endTime).TimeOfDay);

            if (!(startDateTime < endDateTime))
            {
                await modal.RespondAsync("Start time must be before end time!", ephemeral: true);
                return;
            }

            Availability availabilityDetails = new Availability(modal.User.Id, new DateTimeOffset(startDateTime).ToUnixTimeSeconds(), new DateTimeOffset(endDateTime).ToUnixTimeSeconds(), recurring);
            await Task.Run(() => success = DatabaseManagement.EditAvailability(oldAvailability, availabilityDetails));

            if (success)
                await modal.RespondAsync(embed: availabilityDetails.EditSucessEmbed(oldAvailability), ephemeral: true);
            else
                await modal.RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);

        }
    }
}
