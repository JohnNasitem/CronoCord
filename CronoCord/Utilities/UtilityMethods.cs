//***********************************************************************************
//Program: Utilities.cs
//Description: Helper functions
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CronoCord
{
    public static class UtilityMethods
    {
        /// <summary>
        /// Matches time of day, eg 3am, 4:00 pm, 22:40.<br/>
        /// Usage $"(?i)^{<see cref="TimeOfDayRegexString"/>}$"
        /// </summary>
        public static string TimeOfDayRegexString { get; } = @"(\d{1,2})(:(\d{2}))?\s?(am|pm)";



        /// <summary>
        /// Matches date<br/>
        /// eg Feb 18, 2025, Feb182025, Feb 18,2025.<br/>
        /// Group 1 (the month) only matches 3 letter characters so pair with <see cref="AbbreviatedMonths"/><br/>
        /// Usage $"(?i)^{<see cref="DateRegexString"/>}$"
        /// </summary>
        public static string DateRegexString { get; } = @"([a-z]{3})\s?(\d{1,2}),?\s?(\d{4})";



        /// <summary>
        /// List of abbreviated months
        /// </summary>
        public static List<string> AbbreviatedMonths = new List<string> { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };



        /// <summary>
        /// Convert a string into <see cref="DateTime?"/>
        /// </summary>
        /// <param name="input">string to parse</param>
        /// <returns>DateTime if parse was successful, null if not</returns>
        public static DateTime? ParseDateTime(string input)
        {
            // Group 1: Full Date
            // Group 2: Month
            // Group 3: Day of the month
            // Group 4: Year
            // Group 5: Full Time of day
            // Group 6: Hour
            // Group 7: NOT USED
            // Group 8: Minutes
            // Group 9: am/pm
            Match dateTimeMatch = Regex.Match(input, $@"(?i)^({DateRegexString})?\s?({TimeOfDayRegexString})?$");

            // Return null if invalid date was inputted
            if (!dateTimeMatch.Success || !AbbreviatedMonths.Contains(dateTimeMatch.Groups[2].ToString().ToLower()))
                return null;

            //Parse data
            int.TryParse(dateTimeMatch.Groups[4].ToString(), out int year);
            int month = AbbreviatedMonths.IndexOf(dateTimeMatch.Groups[2].ToString().ToLower()) + 1;
            int.TryParse(dateTimeMatch.Groups[3].ToString(), out int dayOfMonth);
            int.TryParse(dateTimeMatch.Groups[6].ToString(), out int hours);
            int.TryParse(dateTimeMatch.Groups[8].ToString(), out int minutes);
            string meridiem = dateTimeMatch.Groups[9].ToString().ToLower();

            if (hours > 12)
                return null;

            if (hours == 12)
                hours = meridiem == "am" ? 0 : 12;
            else
                hours += meridiem == "am" ? 0 : 12;

            try
            {
                return new DateTime(year, month, dayOfMonth, hours, minutes, 0);
            }
            catch
            {
                return null;
            }
        }
    }
}
