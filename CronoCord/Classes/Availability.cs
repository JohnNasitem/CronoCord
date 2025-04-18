﻿//***********************************************************************************
//Program: Availability.cs
//Description: Availability details
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Discord;

namespace CronoCord.Classes
{
    public class Availability
    {
        /// <summary>
        /// ID of user who created the availability
        /// </summary>
        public ulong UserID { get; }
        /// <summary>
        /// Start date time in epic unix timestamp
        /// </summary>
        public long StartTimeUnix { get; }
        /// <summary>
        /// End date time in epic unix timestamp
        /// </summary>
        public long EndTimeUnix { get; }
        /// <summary>
        /// Is availability recurring
        /// </summary>
        public Recurring IsRecurring { get; }



        /// <summary>
        /// Availability recurring states
        /// </summary>
        public enum Recurring
        {
            N,
            D,
            W,
            M,
            Y
        }


        private readonly Dictionary<Recurring, string> _expandedRecurring = new Dictionary<Recurring, string>()
        {
            {Recurring.N, "Never" },
            {Recurring.D, "Daily" },
            {Recurring.W, "Weekly" },
            {Recurring.M, "Monthly" },
            {Recurring.Y, "Yearly" }
        };



        /// <summary>
        /// Initializes a new instance of the <see cref="Availability"/> class.
        /// </summary>
        /// <param name="userId">Id of user who created the availability</param>
        /// <param name="startTimeUnix">epoch unix timestamp for start date</param>
        /// <param name="endTimeUnix">epoch unix timestamp for end date</param>
        /// <param name="recurring">does the availability re-occur</param>
        public Availability(ulong userId, long startTimeUnix, long endTimeUnix, Recurring recurring)
        {
            UserID = userId;
            StartTimeUnix = startTimeUnix;
            EndTimeUnix = endTimeUnix;
            IsRecurring = recurring;
        }



        /// <summary>
        /// Create an embed using instance data
        /// </summary>
        /// <returns>Discord embed</returns>
        public Embed CreateSucessEmbed()
        {
            Embed embed = new EmbedBuilder()
                    .WithTitle($"Successfully created availability slot!")
                    .WithDescription($"Date: {UtilityMethods.ToUnixTimeStamp(StartTimeUnix, "D")}\n" +
                                     $"Start Time: {UtilityMethods.ToUnixTimeStamp(StartTimeUnix, "t")}\n" +
                                     $"End Time: {UtilityMethods.ToUnixTimeStamp(EndTimeUnix, "t")}\n" +
                                     $"Recurring: {_expandedRecurring[IsRecurring]}")
                    .WithColor(Color.Green)
                    .AddField("Other Commands", "Use /edit-schedule to edit your availability slots\nUse /view-schedule to view your schedule", false)
                    .Build();
            return embed;
        }




        /// <summary>
        /// Create an embed displaying before and after
        /// </summary>
        /// <returns>Discord embed</returns>
        public Embed EditSucessEmbed(Availability oldAvailability)
        {
            Embed embed = new EmbedBuilder()
                    .WithTitle($"Successfully editted availability slot!")
                    .WithDescription($"Date: {UtilityMethods.ToUnixTimeStamp(oldAvailability.StartTimeUnix, "D")} -> {UtilityMethods.ToUnixTimeStamp(StartTimeUnix, "D")}\n" +
                                     $"Start Time: {UtilityMethods.ToUnixTimeStamp(oldAvailability.StartTimeUnix, "t")} -> {UtilityMethods.ToUnixTimeStamp(StartTimeUnix, "t")}\n" +
                                     $"End Time: {UtilityMethods.ToUnixTimeStamp(oldAvailability.EndTimeUnix, "t")} -> {UtilityMethods.ToUnixTimeStamp(EndTimeUnix, "t")}\n" +
                                     $"Recurring: {_expandedRecurring[oldAvailability.IsRecurring]} -> {_expandedRecurring[IsRecurring]}")
                    .WithColor(Color.Green)
                    .Build();
            return embed;
        }



        /// <summary>
        /// Logical equality
        /// </summary>
        /// <param name="obj">object to compare to</param>
        /// <returns>true if specified object is logically equal</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Availability other))
                return false;

            // Logically equality
            return UserID.Equals(other.UserID) && StartTimeUnix.Equals(other.StartTimeUnix) && EndTimeUnix.Equals(other.EndTimeUnix) && IsRecurring.Equals(other.IsRecurring);
        }



        /// <summary>
        /// Checks whether this instance's start and end times overlaps with another instance's start and end times
        /// </summary>
        /// <param name="other">Other instance</param>
        /// <returns>true if this instance does overlap with the specified instance</returns>
        public bool Overlaps(Availability other)
        {
            return other.StartTimeUnix <= EndTimeUnix && StartTimeUnix <= other.EndTimeUnix;
        }



        /// <summary>
        /// Return 1
        /// </summary>
        /// <returns>1</returns>
        public override int GetHashCode()
        {
            return 1;
        }



        /// <summary>
        /// Returns a string that represents the current object
        /// </summary>
        /// <returns>a string that represents the current object</returns>
        public override string ToString()
        {
            return $"UserID: {UserID} - StartTimeUnix: {StartTimeUnix} - EndTimeUnix: {EndTimeUnix} - Recurring: {Enum.GetName(typeof(Recurring), IsRecurring)}";
        }
    }
}
