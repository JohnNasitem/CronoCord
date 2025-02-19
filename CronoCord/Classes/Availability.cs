//***********************************************************************************
//Program: Availability.cs
//Description: Availability details
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using Discord;
using System;
using System.Collections.Generic;
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
                    .WithColor(Color.Green)
                    .Build();
            return embed;
        }
    }
}
