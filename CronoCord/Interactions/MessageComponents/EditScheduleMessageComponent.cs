//***********************************************************************************
//Program: EditScheduleMessageComponent.cs
//Description: Edit availability message
//Date: Feb 21, 2025
//Author: John Nasitem
//***********************************************************************************



using CronoCord.Classes;
using CronoCord.Utilities;
using Discord;
using Discord.WebSocket;

namespace CronoCord.Interactions.MessageComponents
{
    public class EditScheduleMessageComponent : ComponentBuilder
    {
        public MessageComponent MessageComponent = null;
        public Embed Embed = null;

        public EditScheduleMessageComponent(List<Availability> availabilitesToDisplay, int amountToDisplay, int offset)
        {
            if (amountToDisplay > 25)
                throw new ArgumentException("amountToDisplay cannot be greater than 25 due to discord limits");

            EmbedBuilder embedMenu = new EmbedBuilder()
                    .WithTitle($"Edit Availability Slots")
                    .WithDescription("Select the availability slot to edit using the drop down menu")
                    .WithColor(Color.Gold);

            SelectMenuBuilder selectMenu = new SelectMenuBuilder()
                    .WithCustomId("edit-schedule-menu")
                    .WithPlaceholder("Select an availability to edit")
                    .WithMaxValues(1)
                    .WithMinValues(1);

            ButtonBuilder previousButton = new ButtonBuilder()
                    .WithCustomId($"edit-schedule-button:previous,{amountToDisplay},{offset}")
                    .WithLabel("Previous")
                    .WithDisabled(offset == 0)
                    .WithStyle(ButtonStyle.Secondary);


            ButtonBuilder nextButton = new ButtonBuilder()
                    .WithCustomId($"edit-schedule-button:next,{amountToDisplay},{offset}")
                    .WithLabel("Next")
                    .WithDisabled(offset + amountToDisplay >= availabilitesToDisplay.Count)
                    .WithStyle(ButtonStyle.Secondary);

            // Populate embed fields and select options with the visible availabilities
            for (int i = offset; i - offset < amountToDisplay; i++)
            {
                // Stop if no more availabilities are left to display
                if (i >= availabilitesToDisplay.Count)
                    break;

                //Convert data
                Availability a = availabilitesToDisplay[i];
                DateTime startTime = UtilityMethods.ToDateTime(a.StartTimeUnix);
                DateTime endTime = UtilityMethods.ToDateTime(a.EndTimeUnix);

                embedMenu.AddField($"{i + 1} - {UtilityMethods.ToUnixTimeStamp(a.StartTimeUnix, "D")}", $"{UtilityMethods.ToUnixTimeStamp(a.StartTimeUnix, "t")} - {UtilityMethods.ToUnixTimeStamp(a.EndTimeUnix, "t")}", false);
                selectMenu.AddOption($"{i + 1} - {startTime.ToString("MMM d yyyy")}", $"{a.StartTimeUnix},{a.EndTimeUnix},{Enum.GetName(typeof(Availability.Recurring), a.IsRecurring)},{i}", $"{startTime.ToString("hh:mm tt")} - {endTime.ToString("hh:mm tt")}");
            }

            ComponentBuilder components = new ComponentBuilder()
                .WithButton(previousButton)
                .WithSelectMenu(selectMenu)
                .WithButton(nextButton);

            MessageComponent = components.Build();
            Embed = embedMenu.Build();
        }



        /// <summary>
        /// Button press handler
        /// </summary>
        /// <param name="arg">SocketMessageComponent</param>
        /// <returns>Task</returns>
        public static async Task ButtonPressed(SocketMessageComponent arg)
        {
            string[] buttonData = arg.Data.CustomId.Split(':')[1].Split(',');
            string direction = buttonData[0];
            int amountToDisplay = int.Parse(buttonData[1]);
            int offset = int.Parse(buttonData[2]);
            List<Availability> userSchedule = null;

            // Get availabilities
            await Task.Run(() => userSchedule = DatabaseManagement.GetAvailabilites(new List<ulong>() { arg.User.Id }));

            if (userSchedule != null)
            {
                // Move pages
                if (userSchedule.Count != 0)
                    offset += direction == "previous" ? -amountToDisplay : amountToDisplay;

                // Generate new page
                EditScheduleMessageComponent menuStuff = new EditScheduleMessageComponent(userSchedule, amountToDisplay, offset);

                // Edit original message
                await arg.DeferAsync();
                await arg.ModifyOriginalResponseAsync(properties =>
                {
                    properties.Embed = menuStuff.Embed;
                    properties.Components = menuStuff.MessageComponent;
                });
            }
            else
                await arg.RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
        }
    }
}
