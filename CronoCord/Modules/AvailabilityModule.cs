//***********************************************************************************
//Program: AvailabilityModule.cs
//Description: Holds Availability commands
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Interactions;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Reflection;
using CronoCord.Utilities;
using CronoCord.Classes;
using System.Text.RegularExpressions;
using System.Windows;
using System.IO;
using System.Drawing;

namespace CronoCord.Modules
{
    public class AvailabilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        private static Random _rand = new Random();
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
            List<Availability> filtered_availabilities = new List<Availability>();
            
            // Get availabilities
            await Task.Run(() => unfiltered_availabilities = DatabaseManagement.GetAvailabilites(users.Select(u => u.Id).ToList()));

            // Null means something went wrong with accessing the database
            if (unfiltered_availabilities == null)
            {
                Console.WriteLine("Method: ViewSchedule - Problem: unfiltered_availabilities was null, something happened with getting availabilities");
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                return;
            }


            foreach (Availability availability in unfiltered_availabilities)
            {
                // Include the availabilities happning this week
                if (UtilityMethods.GetSundayUnixTimeStamp(true) <= availability.StartTimeUnix && availability.StartTimeUnix < UtilityMethods.GetSundayUnixTimeStamp(false))
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

            GenerateScheduleImage(filtered_availabilities, weekOffset, showOverlapCount);
            await RespondWithFileAsync("schedule.png", text: "Here is your schedule!");
        }

        private void GenerateScheduleImage(List<Availability> availabilities, int weekOffset, bool showOverlapCount)
        {
            // TODO: this breaks when availabilities is empty
            List<ulong> uniqueIDs = availabilities.Select(a => a.UserID).Distinct().ToList();
            Dictionary<ulong, System.Drawing.Color> userColours = GenerateUserColours(uniqueIDs);

            //Does it need to be sorted?
            availabilities.Sort((a1, a2) => a1.StartTimeUnix.CompareTo(a2.StartTimeUnix));

            int backgroundWidth = 2300;
            System.Drawing.Size imageSize = new System.Drawing.Size(uniqueIDs.Count == 0 ? backgroundWidth : backgroundWidth + 400, 2500);

            // Create schedule bitmap
            using (Bitmap bm = new Bitmap(imageSize.Width, imageSize.Height))
            {
                // Create a Graphics object from the bitmap
                using (Graphics graphics = Graphics.FromImage(bm))
                {
                    // Set the background color to white
                    graphics.Clear(System.Drawing.Color.White);

                    // Draw a semi-transparent red rectangle
                    using (Brush redBrush = new SolidBrush(System.Drawing.Color.FromArgb(128, 255, 0, 0))) // Alpha = 128
                    {
                        graphics.FillRectangle(redBrush, 100, 100, 200, 150);
                    }

                    // Draw a blue line
                    using (Pen bluePen = new Pen(System.Drawing.Color.Blue, 5)) // Width = 5
                    {
                        graphics.DrawLine(bluePen, 50, 50, 300, 300);
                    }

                    // Draw some text
                    using (Font font = new Font("Arial", 24))
                    using (Brush textBrush = new SolidBrush(System.Drawing.Color.Black))
                    {
                        graphics.DrawString("Hello, World!", font, textBrush, new PointF(50, 400));
                    }
                }
                bm.Save("schedule.png", System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        /// <summary>
        /// Generate a unique color for each user
        /// </summary>
        /// <param name="userIDs">List of user ids to get colours for</param>
        /// <returns>Dictionary with user id as key and color as value</returns>
        private Dictionary<ulong, System.Drawing.Color> GenerateUserColours(List<ulong> userIDs)
        {
            // Generate a "light" colour
            System.Drawing.Color GenerateColour()
            {
                while (true)
                {
                    byte r = (byte)_rand.Next(180, 256);
                    byte g = (byte)_rand.Next(180, 256);
                    byte b = (byte)_rand.Next(180, 256);
                    // Avoid colors with too similar RGB values (to skip gray-like tones)
                    // Avoid brownish hues by ensuring no dominant mix of red and green
                    if (Math.Abs(r - g) > 30 || Math.Abs(r - b) > 30 || Math.Abs(g - b) > 30 && !(r > 200 && 210 < g && g > 150 && b < 100))
                        return System.Drawing.Color.FromArgb (128, r, g, b);
                }
            }


            Dictionary<ulong, System.Drawing.Color> userColours = new Dictionary<ulong, System.Drawing.Color>();

            // Generate unique colours for each user
            foreach (ulong userID in userIDs)
            {
                System.Drawing.Color possibleColour = System.Drawing.Color.Empty;
                do possibleColour = GenerateColour();
                while (userColours.ContainsValue(possibleColour));
                userColours.Add(userID, possibleColour);
            }

            return userColours;
        }
    }
}
