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
using SkiaSharp;
using System.Windows;
using System.IO;


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
            //try
            //{
            //    // List of users
            //    List<IUser> users = new List<IUser>();

            //    // Find all users from the usersStr
            //    foreach (Match match in Regex.Matches(usersStr, @"\d+"))
            //    {
            //        IUser possibleUser = Context.Client.GetUserAsync(ulong.Parse(match.Value)).Result;

            //        if (possibleUser != null)
            //            users.Add(possibleUser);
            //    }

            //    // Default to invoking user
            //    if (usersStr == "")
            //        users.Add(Context.User);

            //    // Only positive week offsets
            //    if (weekOffset < 0)
            //        weekOffset = 0;

            //    await ViewSchedule(users, weekOffset, showOverlapCount);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Method: ViewScheduleCommand - Problem: {ex}");
            //    await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
            //}

        }

        private async Task ViewSchedule(List<IUser> users, int weekOffset, bool showOverlapCount)
        {
            List<Availability> unfiltered_availabilities = null;
            List<Availability> filtered_availabilities = null;

            Console.WriteLine("ViewSchedule - 1");

            await Task.Run(() => unfiltered_availabilities = DatabaseManagement.GetAvailabilites(users.Select(u => u.Id).ToList()));

            Console.WriteLine("ViewSchedule - 2");

            if (unfiltered_availabilities == null)
            {
                Console.WriteLine("ViewSchedule - 3");
                Console.WriteLine("Method: ViewSchedule - Problem: unfiltered_availabilities was null, something happened with getting availabilities");
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                return;
            }


            Console.WriteLine("ViewSchedule - 4");

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

            Console.WriteLine("ViewSchedule - 5");

            GenerateScheduleImage(filtered_availabilities, weekOffset, showOverlapCount);

            Console.WriteLine("ViewSchedule - 6");
            await RespondWithFileAsync("schedule.png", text: "Here is your schedule!");

            Console.WriteLine("ViewSchedule - 7");
            //try
            //{
            //    GenerateScheduleImage(filtered_availabilities, weekOffset, showOverlapCount);
            //    await RespondWithFileAsync("schedule.png", text: "Here is your schedule!");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Problem: {ex}");
            //}
        }

        private void GenerateScheduleImage(List<Availability> availabilities, int weekOffset, bool showOverlapCount)
        {
            Console.WriteLine("GenerateScheduleImage - 1");
            // TODO: this breaks when availabilities is empty
            List<ulong> uniqueIDs = availabilities.Select(a => a.UserID).Distinct().ToList();
            Console.WriteLine("GenerateScheduleImage - 2");
            Dictionary<ulong, SKPaint> userColours = GenerateUserColours(uniqueIDs);
            Console.WriteLine("GenerateScheduleImage - 3");

            //Does it need to be sorted?
            availabilities.Sort((a1, a2) => a1.StartTimeUnix.CompareTo(a2.StartTimeUnix));
            Console.WriteLine("GenerateScheduleImage - 4");

            int backgroundWidth = 2300;
            SKImageInfo imageSize = new SKImageInfo(uniqueIDs.Count == 0 ? backgroundWidth : backgroundWidth + 400, 2500);

            Console.WriteLine("GenerateScheduleImage - 5");

            using (SKSurface surface = SKSurface.Create(imageSize))
            {
                Console.WriteLine("GenerateScheduleImage - 6");
                SKCanvas canvas = surface.Canvas;
                canvas.Clear();

                canvas.DrawRect(0, 0, 200, 200, userColours[uniqueIDs[0]]);

                Console.WriteLine("GenerateScheduleImage - 7");

                // save the file
                SKData data = surface.Snapshot().Encode(SKEncodedImageFormat.Png, 100);

                Console.WriteLine("GenerateScheduleImage - 8");
                var stream = File.OpenWrite("schedule.png");
                Console.WriteLine("GenerateScheduleImage - 9");
                data.SaveTo(stream);
                Console.WriteLine("GenerateScheduleImage - 10");
            }
        }



        /// <summary>
        /// Generate a unique color for each user
        /// </summary>
        /// <param name="userIDs">List of user ids to get colours for</param>
        /// <returns>Dictionary with user id as key and color as value</returns>
        private Dictionary<ulong, SKPaint> GenerateUserColours(List<ulong> userIDs)
        {
            // Generate a "light" colour
            SKPaint GenerateColour()
            {
                while (true)
                {
                    byte r = (byte)_rand.Next(180, 256);
                    byte g = (byte)_rand.Next(180, 256);
                    byte b = (byte)_rand.Next(180, 256);

                    // Avoid colors with too similar RGB values (to skip gray-like tones)
                    // Avoid brownish hues by ensuring no dominant mix of red and green
                    if (Math.Abs(r - g) > 30 || Math.Abs(r - b) > 30 || Math.Abs(g -b) > 30 && !(r > 200 && 210 < g && g > 150 && b< 100))
                        return new SKPaint{
                            Color = new SKColor(r, g, b, 128),
                            IsAntialias = true
                        };
                }
            }

            Dictionary<ulong, SKPaint> userColours = new Dictionary<ulong, SKPaint>();

            // Generate unique colours for each user
            foreach (ulong userID in userIDs)
            {
                SKPaint possibleColour = null;

                do possibleColour = GenerateColour();
                while (!userColours.ContainsValue(possibleColour));

                userColours.Add(userID, possibleColour);
            }

            return userColours;
        }
    }
}
