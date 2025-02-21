//***********************************************************************************
//Program: AvailabilityModule.cs
//Description: Holds Availability commands
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Discord.Interactions;
using Discord;
using CronoCord.Utilities;
using CronoCord.Classes;
using System.Text.RegularExpressions;
using CronoCord.Interactions.MessageComponents;
using SkiaSharp;

namespace CronoCord.Modules
{
    public class AvailabilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Random to generate colours
        private readonly static Random _rand = new Random();
        // Font text for the schedule image
        private readonly SKFont COLHEADERFONT = new SKFont
        {
            Size = 45,
            Typeface = SKTypeface.FromFamilyName("Arial"),
        };
        private readonly SKFont ROWHEADERFONT = new SKFont
        {
            Size = 30,
            Typeface = SKTypeface.FromFamilyName("Arial"),
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="AvailabilityModule"/>
        /// </summary>
        public AvailabilityModule() { }



        /// <summary>
        /// Adds a new availability slot
        /// </summary>
        /// <returns></returns>
        [SlashCommand("add-availability-slot", "Add an availability slot to your schedule")]
        public async Task AddAvailabilitySlotCommand()
        {
            try
            {
                await RespondWithModalAsync(new Interactions.Modals.CreateAvailabilityModal().Build());
            }
            catch (Exception ex)
            {
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in AddAvailabilitySlotCommand(). Exception: {ex}", UtilityMethods.LogLevel.Error);
            }
        }



        /// <summary>
        /// Views the availability slots of the specified users
        /// </summary>
        /// <param name="usersStr">Users to view</param>
        /// <param name="weekOffset">Offset from current week</param>
        /// <param name="showOverlapCount">should overlap count be shown</param>
        /// <returns></returns>
        [SlashCommand("view-schedule", "View schedules")]
        public async Task ViewScheduleCommand(
            [Summary("users", "Mention the users you want to view availabilities for. eg @CronoCord")] string usersStr = "",
            [Summary("week_offset", "How many weeks ahead to view. eg 1 means next week")] int weekOffset = 0,
            [Summary("show_overlap-Count", "Show how many slots are overlapping each cell.")] bool showOverlapCount = false)
        {
            try
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
            catch (Exception ex)
            {
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in ViewScheduleCommand(). Exception: {ex}", UtilityMethods.LogLevel.Error);
            }
        }



        /// <summary>
        /// Edits the users schedule
        /// </summary>
        /// <returns></returns>
        [SlashCommand("edit-schedule", "Edit your schedule availabilities")]
        public async Task EditScheduleCommand()
        {
            try
            {
                List<Availability> userSchedule = null;

                // Get availabilities
                await Task.Run(() => userSchedule = DatabaseManagement.GetAvailabilites(new List<ulong>() { Context.User.Id }));

                if (userSchedule.Count == 0)
                    await RespondAsync($"No availabilities to edit! Use the /add-availability-slot to add one", ephemeral: true);
                else
                {
                    // Generate edit availability message and send it to user
                    EditScheduleMessageComponent menuStuff = new EditScheduleMessageComponent(userSchedule, 5, 0);
                    await RespondAsync(embed: menuStuff.Embed, components: menuStuff.MessageComponent, ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                UtilityMethods.PrettyConsoleWriteLine($"Problem occured in ViewScheduleCommand(). Exception: {ex}", UtilityMethods.LogLevel.Error);
            }
        }



        /// <summary>
        /// View the schedule of the specified week with specified users
        /// </summary>
        /// <param name="users">Users availabilities to view</param>
        /// <param name="weekOffset">Week number offset from current week</param>
        /// <param name="showOverlapCount">Should overlap count be shown</param>
        /// <returns></returns>
        private async Task ViewSchedule(List<IUser> users, int weekOffset, bool showOverlapCount)
        {
            List<Availability> unfiltered_availabilities = null;
            List<Availability> filtered_availabilities = new List<Availability>();
            
            // Get availabilities
            await Task.Run(() => unfiltered_availabilities = DatabaseManagement.GetAvailabilites(users.Select(u => u.Id).ToList()));

            // Null means something went wrong with accessing the database
            if (unfiltered_availabilities == null)
            {
                UtilityMethods.PrettyConsoleWriteLine($"Method: ViewSchedule - Problem: unfiltered_availabilities was null, something happened with getting availabilities for this users: {string.Join(", ", users.Select(u => u.Id))}", UtilityMethods.LogLevel.Error);
                await RespondAsync($"Something went wrong... contact <@{Program.AuthorID}>", ephemeral: true);
                return;
            }

            // Filter out any availabilities dont fall within the offsetted week range and arent recurring 
            // For the recurring availabilites only add the recurrences that do fall within the week range 
            foreach (Availability availability in unfiltered_availabilities)
            {
                DateTime offsettedDate = DateTime.Now.AddDays(weekOffset * 7);

                // Get selected week's ranges
                long startSundayUnix = UtilityMethods.GetSundayUnixTimeStamp(true, offsettedDate);
                long endSaturdayUnix = UtilityMethods.GetSundayUnixTimeStamp(false, offsettedDate) - 1;

                // Include the availabilities happning this week that arent repeating
                if (startSundayUnix <= availability.StartTimeUnix && availability.EndTimeUnix <= endSaturdayUnix)
                    filtered_availabilities.Add(availability);

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
                            int monthsDiff = (offsettedDate.Year - originalDateM.Year) * 12 + (offsettedDate.Month - originalDateM.Month);

                            // Get the startTimeunix and endTimeUnix for this month
                            long offsetStartUnixM = new DateTimeOffset(UtilityMethods.ToDateTime(availability.StartTimeUnix).AddMonths(monthsDiff)).ToUnixTimeSeconds();
                            long offsetEndUnixM = new DateTimeOffset(UtilityMethods.ToDateTime(availability.EndTimeUnix).AddMonths(monthsDiff)).ToUnixTimeSeconds();

                            // Add slot if it is within the selected week's range
                            if (startSundayUnix <= offsetStartUnixM && offsetStartUnixM <= endSaturdayUnix)
                                filtered_availabilities.Add(new Availability(availability.UserID, offsetStartUnixM, offsetEndUnixM, Availability.Recurring.N));
                            break;
                        case Availability.Recurring.Y:
                            DateTime originalDateY = UtilityMethods.ToDateTime(availability.StartTimeUnix);
                            int yearDiff = offsettedDate.Year - originalDateY.Year;

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



        /// <summary>
        /// Generates a schedule image in the file path "schedule.png"
        /// </summary>
        /// <param name="mentionedUsers">mentioned users</param>
        /// <param name="availabilities">availability slots</param>
        /// <param name="weekOffset">week offset from current week</param>
        /// <param name="showOverlapCount">Should overlap count be shown</param>
        private void GenerateScheduleImage(List<IUser> mentionedUsers, List<Availability> availabilities, int weekOffset, bool showOverlapCount)
        {
            List<ulong> mentionedUsersID = mentionedUsers.Select(u => u.Id).ToList();
            Dictionary<ulong, SKPaint> userColours = GenerateUserColours(mentionedUsersID);
            int backgroundWidth = 2300;
            SKPaint textPaint = new SKPaint()
            {
                Color = SKColors.Black
            };

            // If there are availabilities then merged overlapping slots
            if (availabilities.Count > 0)
                availabilities = MergeOverlappingSlots(availabilities);

            // Create image size
            SKImageInfo imageSize = new SKImageInfo(mentionedUsersID.Count < 2 ? backgroundWidth : backgroundWidth + 400, 2500);

            // Create schedule image
            using (SKSurface surface = SKSurface.Create(imageSize))
            using (SKCanvas canvas = surface.Canvas)
            {
                // Set the background color to white
                canvas.Clear(SKColors.White);

                // Populate row headers and draw horizontal lines
                for (int rowIndex = 0; rowIndex < 48; rowIndex++)
                {
                    //Add row header
                    string text = (rowIndex % 24 / 2).ToString();
                    if (text == "0")
                        text = "12";

                    text += rowIndex % 2 == 0 ? ":00" : ":30";
                    text += rowIndex < 24 ? " am" : " pm";

                    canvas.DrawText(text, new SKPoint(100, 135 + (rowIndex * 50)), SKTextAlign.Center, ROWHEADERFONT, textPaint);

                    // Add horizontal line
                    canvas.DrawLine(0, 100 + (rowIndex * 50), backgroundWidth, 100 + (rowIndex * 50), textPaint);
                }

                // Unix for the sunday of the specified week
                long thisWeekSundayUnix = UtilityMethods.GetSundayUnixTimeStamp(true, DateTime.Now.AddDays(weekOffset * 7));
                // Populate column headers and draw vertical lines
                foreach (DayOfWeek day in Enum.GetValues(typeof(DayOfWeek)))
                {
                    // Add date column header
                    canvas.DrawText(UtilityMethods.ToDateTime(thisWeekSundayUnix + ((int)day * 86400)).ToString("MMM d"), 
                                    new SKPoint(350 + ((int)day * 300), 35), SKTextAlign.Center, COLHEADERFONT, textPaint);

                    // Add date of the week column header
                    canvas.DrawText(Enum.GetName(typeof(DayOfWeek), day), new SKPoint(350 + ((int)day * 300), 75), SKTextAlign.Center, COLHEADERFONT, textPaint);

                    //Add vertical line
                    canvas.DrawLine(200 + ((int)day * 300), 0, 200 + ((int)day * 300), imageSize.Height, textPaint);
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
                    canvas.DrawRect(new SKRect(200 + ((int)date.DayOfWeek * 300), yPositions[0], 500 + ((int)date.DayOfWeek * 300), yPositions[1]), userColours[availability.UserID]);
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
                            int count = availabilities.Count(a => GetTimeIndex(UtilityMethods.ToDateTime(a.StartTimeUnix).ToString("hh:mm tt")) <= rowIndex
                                                                && rowIndex < GetTimeIndex(UtilityMethods.ToDateTime(a.EndTimeUnix).ToString("hh:mm tt"))
                                                                && UtilityMethods.ToDateTime(a.StartTimeUnix).DayOfWeek == day);

                            // Add count text if count isnt 0
                            if (count > 0)
                                canvas.DrawText(count.ToString(), new SKPoint(350 + ((int)day * 300), 135 + (rowIndex * 50)), SKTextAlign.Center, ROWHEADERFONT, textPaint);
                        }
                    }
                }

                // Display color legend
                if (mentionedUsers.Count > 0)
                {
                    // Add another vertical line to seperate legend from schedule
                    canvas.DrawLine(200 + (7 * 300), 0, 200 + (7 * 300), imageSize.Height, textPaint);

                    for (int i = 0; i < mentionedUsers.Count; i++)
                    {
                        IUser user = mentionedUsers[i];

                        // Add rectangle
                        canvas.DrawRect(new SKRect(backgroundWidth, i * 50, backgroundWidth + 400, 50 + (i * 50)), userColours[user.Id]);

                        // Add display name
                        canvas.DrawText(user.GlobalName, new SKPoint(backgroundWidth + 200, 35 + (i * 50)), SKTextAlign.Center, ROWHEADERFONT, textPaint);
                    }
                }
                try
                {
                    using (var image = surface.Snapshot())
                    using (var data = image.Encode())
                    using (var stream = new FileStream("schedule.png", FileMode.Create))
                    {
                        data.SaveTo(stream);  // Save the data to the file stream
                    }
                }
                catch (Exception ex)
                {
                    UtilityMethods.PrettyConsoleWriteLine($"Method: GenerateScheduleImage() - Problem: Couldn't save the generated image - {ex}", UtilityMethods.LogLevel.Error);
                }
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
                    if (Math.Abs(r - g) > 30 || Math.Abs(r - b) > 30 || Math.Abs(g - b) > 30 && !(r > 200 && 210 < g && g > 150 && b < 100))
                        return new SKPaint
                        {
                            Color = new SKColor(r, g, b, 128),
                            IsAntialias = true
                        };
                }
            }


            Dictionary<ulong, SKPaint> userColours = new Dictionary<ulong, SKPaint>();

            // Generate unique colours for each user
            foreach (ulong userID in userIDs)
            {
                SKPaint possibleColour;
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

            int minutes = int.Parse(timeMatch.Groups[3].Value);

            if (15 <= minutes && minutes < 45)
                timeIndex += 1;
            else if (45 <= minutes && minutes < 60)
                timeIndex += 2;
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
