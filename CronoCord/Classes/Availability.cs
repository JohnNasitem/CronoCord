//***********************************************************************************
//Program: Availability.cs
//Description: Availability details
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Discord;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Embed CreateEmbed()
        {
            Dictionary<Recurring, string> expandedRecurring = new Dictionary<Recurring, string>()
            {
                {Recurring.N, "Never" },
                {Recurring.D, "Daily" },
                {Recurring.W, "Weekly" },
                {Recurring.M, "Monthly" },
                {Recurring.Y, "Yearly" }
            };

            Embed embed = new EmbedBuilder()
                    .WithTitle($"Successfully created availability slot!")
                    .WithDescription($"Date: {UtilityMethods.ToUnixTimeStamp(StartTimeUnix, "D")}\n" +
                                     $"Start Time: {UtilityMethods.ToUnixTimeStamp(StartTimeUnix, "t")}\n" +
                                     $"End Time: {UtilityMethods.ToUnixTimeStamp(EndTimeUnix, "t")}\n" +
                                     $"Recurring: {expandedRecurring[IsRecurring]}")
                    .WithColor(Discord.Color.Green)
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



        public override string ToString()
        {
            return $"UserID: {UserID} - StartTimeUnix: {StartTimeUnix} - EndTimeUnix: {EndTimeUnix} - Recurring: {Enum.GetName(typeof(Recurring), IsRecurring)}";
        }
    }
}
