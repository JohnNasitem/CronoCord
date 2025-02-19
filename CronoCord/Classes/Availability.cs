//***********************************************************************************
//Program: Availability.cs
//Description: Availability details
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



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
            No,
            Daily,
            Weekly,
            Monthly,
            Yearly
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
    }

}
