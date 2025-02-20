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
using System.ComponentModel;
using System.Reactive;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace CronoCord.Modules
{
    public class AvailabilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        private static Random _rand = new Random();
        private readonly Font COLHEADERFONT = new Font("Arial", 35);
        private readonly Font ROWHEADERFONT = new Font("Arial", 30);

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
                DateTime offsetedDate = DateTime.Now.AddDays(weekOffset * 7);

                // Get selected week's ranges
                long startSundayUnix = UtilityMethods.GetSundayUnixTimeStamp(true, offsetedDate);
                long endSaturdayUnix = UtilityMethods.GetSundayUnixTimeStamp(false, offsetedDate) - 1;

                // Include the availabilities happning this week that arent repeating
                if (startSundayUnix <= availability.StartTimeUnix && availability.EndTimeUnix <= endSaturdayUnix)
                    filtered_availabilities.Add(availability);

                // TODO: Test to make sure that this add a dup with original date
                // Including repeating availabilities
                if (availability.IsRecurring != Availability.Recurring.N && availability.StartTimeUnix < endSaturdayUnix)
                {
                    switch (availability.IsRecurring)
                    {
                        case Availability.Recurring.D:
                            // Skip original day as it is already handled
                            long startTimeUnix = availability.StartTimeUnix + 86_400;
                            long endTimeUnix = availability.EndTimeUnix + 86_400;

                            // If original start date is before selected week's sunday then use sunday as start date
                            if (availability.StartTimeUnix < startSundayUnix)
                            {
                                // Add time of day to startSundayUnix
                                startTimeUnix = startSundayUnix + (int)UtilityMethods.ToDateTime(availability.StartTimeUnix).TimeOfDay.TotalSeconds;
                                endTimeUnix = startSundayUnix + (int)UtilityMethods.ToDateTime(availability.EndTimeUnix).TimeOfDay.TotalSeconds;
                            }

                            for (int i = 0; i < 7; i++)
                            {
                                // Break if the date that is gonna be added is not in selected week's range
                                if (startTimeUnix + (86_400 * i) > endSaturdayUnix)
                                    break;

                                filtered_availabilities.Add(new Availability(availability.UserID, startTimeUnix + (86_400 * i), endTimeUnix + (86_400 * i), Availability.Recurring.N));
                            }
                            break;

                        case Availability.Recurring.W:
                            // Dont add a new availability if the original is still in selected week's range
                            if (availability.StartTimeUnix < startSundayUnix)
                            {
                                int daysFromSunday = (int)((availability.StartTimeUnix + 4 * 86_400) % 604_800 / 86_400);
                                filtered_availabilities.Add(new Availability(availability.UserID,
                                                                             // For both start and end time add the day and time offset to the selected week's sunday
                                                                             startSundayUnix + (daysFromSunday * 86_400) + (int)UtilityMethods.ToDateTime(availability.StartTimeUnix).TimeOfDay.TotalSeconds,
                                                                             startSundayUnix + (daysFromSunday * 86_400) + (int)UtilityMethods.ToDateTime(availability.EndTimeUnix).TimeOfDay.TotalSeconds,
                                                                             Availability.Recurring.N));
                            }
                            break;
                        case Availability.Recurring.M:
                            DateTime originalDateM = UtilityMethods.ToDateTime(availability.StartTimeUnix);
                            int monthsDiff = (offsetedDate.Year - originalDateM.Year) * 12 + (offsetedDate.Month - originalDateM.Month);

                            // Get the startTimeunix and endTimeUnix for this month
                            long offsetStartUnixM = new DateTimeOffset(UtilityMethods.ToDateTime(availability.StartTimeUnix).AddMonths(monthsDiff)).ToUnixTimeSeconds();
                            long offsetEndUnixM = new DateTimeOffset(UtilityMethods.ToDateTime(availability.EndTimeUnix).AddMonths(monthsDiff)).ToUnixTimeSeconds();

                            // Add slot if it is within the selected week's range
                            if (startSundayUnix <= offsetStartUnixM && offsetStartUnixM <= endSaturdayUnix)
                                filtered_availabilities.Add(new Availability(availability.UserID, offsetStartUnixM, offsetEndUnixM, Availability.Recurring.N));
                            break;
                        case Availability.Recurring.Y:
                            DateTime originalDateY = UtilityMethods.ToDateTime(availability.StartTimeUnix);
                            int yearDiff = offsetedDate.Year - originalDateY.Year;

                            // Get the startTimeunix and endTimeUnix for this month
                            long offsetStartUnixY = new DateTimeOffset(UtilityMethods.ToDateTime(availability.StartTimeUnix).AddYears(yearDiff)).ToUnixTimeSeconds();
                            long offsetEndUnixY = new DateTimeOffset(UtilityMethods.ToDateTime(availability.EndTimeUnix).AddYears(yearDiff)).ToUnixTimeSeconds();

                            // Add slot if it is within the selected week's range
                            if (startSundayUnix <= offsetStartUnixY && offsetStartUnixY <= endSaturdayUnix)
                                filtered_availabilities.Add(new Availability(availability.UserID, offsetStartUnixY, offsetEndUnixY, Availability.Recurring.N));
                            break;
                    }
                }
            }

            GenerateScheduleImage(users, filtered_availabilities, weekOffset, showOverlapCount);
            await RespondWithFileAsync("schedule.png", text: "Here is your schedule!");
        }

        private void GenerateScheduleImage(List<IUser> mentionedUsers, List<Availability> availabilities, int weekOffset, bool showOverlapCount)
        {
            List<ulong> mentionedUsersID = mentionedUsers.Select(u => u.Id).ToList();
            Dictionary<ulong, SolidBrush> userColours = null;
            int backgroundWidth = 2300;
            if (availabilities.Count > 0)
            {
                availabilities = MergeOverlappingSlots(availabilities);
                userColours = GenerateUserColours(mentionedUsersID).Select(kvp => new KeyValuePair<ulong, SolidBrush>(kvp.Key, new SolidBrush(kvp.Value))).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                //Does it need to be sorted?
                availabilities.Sort((a1, a2) => a1.StartTimeUnix.CompareTo(a2.StartTimeUnix));
            }

            System.Drawing.Size imageSize = new System.Drawing.Size(mentionedUsersID.Count < 2 ? backgroundWidth : backgroundWidth + 400, 2500);

            // Create schedule bitmap
            using (Bitmap bm = new Bitmap(imageSize.Width, imageSize.Height))
            {
                // Create a Graphics object from the bitmap
                using (Graphics graphics = Graphics.FromImage(bm))
                {
                    // Set the background color to white
                    graphics.Clear(System.Drawing.Color.White);

                    // Populate row headers and draw horizontal lines
                    for (int rowIndex = 0; rowIndex < 48; rowIndex++)
                    {
                        //Add row header
                        string text = (rowIndex % 24 / 2).ToString();
                        if (text == "0")
                            text = "12";

                        text += rowIndex % 2 == 0 ? ":00" : ":30";
                        text += rowIndex < 24 ? " am" : " pm";
                        SizeF textSize = graphics.MeasureString(text, ROWHEADERFONT);
                        PointF textPos = new PointF(100 - textSize.Width / 2, 135 + (rowIndex * 50) - textSize.Height / 2);
                        graphics.DrawString(text, ROWHEADERFONT, Brushes.Black, textPos);

                        // Add horizontal line
                        graphics.DrawLine(Pens.Black, 0, 100 + (rowIndex * 50), backgroundWidth, 100 + (rowIndex * 50));
                    }

                    // Unix for the sunday of the specified week
                    long thisWeekSundayUnix = UtilityMethods.GetSundayUnixTimeStamp(true, DateTime.Now.AddDays(weekOffset * 7));
                    // Populate column headers and draw vertical lines
                    foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                    {
                        // Add date column header
                        string dateText = UtilityMethods.ToDateTime(thisWeekSundayUnix + ((int)day * 86400)).ToString("MMM d");
                        SizeF dateTextSize = graphics.MeasureString(dateText, COLHEADERFONT);
                        PointF datetTextPos = new PointF(350 + ((int)day * 300) - dateTextSize.Width / 2, 25 - dateTextSize.Height / 2);
                        graphics.DrawString(dateText, COLHEADERFONT, Brushes.Black, datetTextPos);

                        // Add date of the week column header
                        string dayText = Enum.GetName(typeof(DayOfWeek), day);
                        SizeF dayTextSize = graphics.MeasureString(dayText, COLHEADERFONT);
                        PointF daytTextPos = new PointF(350 + ((int)day * 300) - dayTextSize.Width / 2, 75 - dayTextSize.Height / 2);
                        graphics.DrawString(dayText, COLHEADERFONT, Brushes.Black, daytTextPos);

                        //Add vertical line
                        graphics.DrawLine(Pens.Black, 200 + ((int)day * 300), 0, 200 + ((int)day * 300), imageSize.Height);
                    }

                    // Add the availabilities slots
                    foreach (Availability availability in availabilities)
                    {
                        // Get y start and end positons
                        List<int> yPositions = new List<int>();
                        DateTime date = UtilityMethods.ToDateTime(availability.StartTimeUnix);
                        foreach (long unix in new long[] { availability.StartTimeUnix, availability.EndTimeUnix })
                            yPositions.Add(100 + (GetTimeIndex(UtilityMethods.ToDateTime(unix).ToString("hh:mm tt")) * 50));

                        //Draw rectangle
                        Rectangle rect = new Rectangle(200 + ((int)date.DayOfWeek * 300), yPositions[0], 300, yPositions[1] - yPositions[0]);
                        graphics.FillRectangle(userColours[availability.UserID], rect);
                    }


                    // Display the number of overlaps in each cell
                    if (showOverlapCount && availabilities.Count > 0)
                    {
                        // Quadratic loop to iterate through each cell
                        foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                        {
                            for (int rowIndex = 0; rowIndex < 48; rowIndex++)
                            {
                                // Count the number of overlaps in this cell that on the same day
                                int count = availabilities.Count(a => GetTimeIndex(UtilityMethods.ToDateTime(a.StartTimeUnix).ToString("HH:MM tt")) <= rowIndex 
                                                                   && rowIndex < GetTimeIndex(UtilityMethods.ToDateTime(a.EndTimeUnix).ToString("HH:MM tt")) 
                                                                   && UtilityMethods.ToDateTime(a.StartTimeUnix).DayOfWeek == day);

                                // Add count text if count isnt 0
                                if (count > 0)
                                {
                                    string dateText = count.ToString();
                                    SizeF dateTextSize = graphics.MeasureString(dateText, ROWHEADERFONT);
                                    PointF datetTextPos = new PointF(350 + ((int)day * 300) - dateTextSize.Width / 2, 135 + (rowIndex * 50) - dateTextSize.Height / 2);
                                    graphics.DrawString(dateText, ROWHEADERFONT, Brushes.Black, datetTextPos);
                                } 
                            }
                        }
                    }

                    // Display color legend
                    if (mentionedUsers.Count > 0)
                    {
                        // Add another vertical line to seperate legend from schedule
                        graphics.DrawLine(Pens.Black, 200 + (7 * 300), 0, 200 + (7* 300), imageSize.Height);

                        for (int i = 0; i < mentionedUsers.Count; i++)
                        {
                            IUser user = mentionedUsers[i];

                            // Add rectangle
                            graphics.FillRectangle(userColours[user.Id], new RectangleF(backgroundWidth, i * 50, 400, 50));

                            // Add display name
                            string dateText = user.GlobalName;
                            SizeF dateTextSize = graphics.MeasureString(dateText, ROWHEADERFONT);
                            PointF datetTextPos = new PointF(backgroundWidth + 200 - dateTextSize.Width / 2, 35 + (i * 50)  - dateTextSize.Height / 2);
                            graphics.DrawString(dateText, ROWHEADERFONT, Brushes.Black, datetTextPos);
                        }
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



        /// <summary>
        /// Convert a time to a time index (row index in the schedule)
        /// </summary>
        /// <param name="time">Time to convert</param>
        /// <returns>time index</returns>
        public static int GetTimeIndex(string time)
        {
            time = time.ToLower().Trim();
            Match timeMatch = Regex.Match(time, UtilityMethods.TimeOfDayRegexString);

            // return -1 if one of the needed groups didnt match
            if (timeMatch.Groups[4].Value == "" || timeMatch.Groups[1].Value == "" || timeMatch.Groups[3].Value == "")
                return -1;

            int timeIndex = timeMatch.Groups[4].Value == "am" ? 0 : 24;
            timeIndex += (int.Parse(timeMatch.Groups[1].Value) % 12) * 2;
            timeIndex += int.Parse(timeMatch.Groups[3].Value) < 30 ? 0 : 1;
            return timeIndex;
        }



        /// <summary>
        /// Merge any slots that are overlapping and are from the same user
        /// </summary>
        /// <param name="unmergedSlots">unmerged availability slots</param>
        /// <returns>merged availability slots</returns>
        public static List<Availability> MergeOverlappingSlots(List<Availability> unmergedSlots)
        {
            // Group availability slots by user and get a list of keys
            Dictionary<ulong, List<Availability>> usersSlots = unmergedSlots.GroupBy(availability => availability.UserID).ToDictionary(group => group.Key, group => group.ToList());
            List<ulong> userIDs = usersSlots.Keys.ToList();

            // Foreach slots under a user, merge overlapping slots
            foreach (ulong userID in userIDs)
            {
                List<Availability> mergedSlots = new List<Availability>();
                List<Availability> unmergedUserSlots = usersSlots[userID];

                // Iterate until all slots have been accounted for
                while (unmergedUserSlots.Count > 0)
                {
                    int mergedSlotCount = 1;
                    Availability currentSlot = unmergedUserSlots[0];

                    // Continue to merge until no more slots overlap with current slot
                    do
                    {
                        List<Availability> slotsToMerge = unmergedUserSlots.Where(other => currentSlot.Overlaps(other)).ToList();
                        mergedSlotCount = unmergedUserSlots.RemoveAll(a => slotsToMerge.Contains(a));
                        currentSlot = new Availability(userID, slotsToMerge.Min(a => a.StartTimeUnix), slotsToMerge.Max(a => a.EndTimeUnix), Availability.Recurring.N);

                        // If at least 2 slots were merged put slot back in to see if new slot will overlap with other
                        if (mergedSlotCount > 1)
                            unmergedUserSlots.Add(currentSlot);
                    }
                    while (mergedSlotCount > 1);

                    mergedSlots.Add(currentSlot);
                }
                usersSlots[userID] = mergedSlots;
            }

            return usersSlots.SelectMany(group => group.Value).ToList();
        }
    }
}
