﻿//***********************************************************************************
//Program: Utilities.cs
//Description: Helper functions
//Date: Feb 18, 2025
//Author: John Nasitem
//***********************************************************************************



using System.Text.RegularExpressions;

namespace CronoCord
{
    public static class UtilityMethods
    {
        /// <summary>
        /// Matches time of day, eg 3am, 4:00 pm, 22:40.<br/>
        /// Group 1: Hour<br/>
        /// Group 2: Not Used<br/>
        /// Group 3: Minutes<br/>
        /// Group 4: am/pm<br/>
        /// Usage $"(?i)^{<see cref="TimeOfDayRegexString"/>}$"
        /// </summary>
        public static string TimeOfDayRegexString { get; } = @"(\d{1,2})(:(\d{2}))?\s?(am|pm)";



        /// <summary>
        /// Matches date<br/>
        /// eg Feb 18, 2025, Feb182025, Feb 18,2025.<br/>
        /// Group 1: Month (only matches 3 letter characters so pair with <see cref="AbbreviatedMonths"/>)<br/>
        /// Group 2: Day of the month<br/>
        /// Group 3: Year<br/>
        /// Usage $"(?i)^{<see cref="DateRegexString"/>}$"
        /// </summary>
        public static string DateRegexString { get; } = @"([a-z]{3})\s?(\d{1,2}),?\s?(\d{4})";



        /// <summary>
        /// Matches date<br/>
        /// eg 02/02/2222, 2/2/2222<br/>
        /// Group 1: Month<br/>
        /// Group 2: Day of the month<br/>
        /// Group 3: Year<br/>
        /// Usage $"(?i)^{<see cref="FormalDateRegexString"/>}$"
        /// Note: numbers still need to be validated
        /// </summary>
        public static string FormalDateRegexString { get; } = @"(\d{1,2})\/(\d{1,2})\/(\d{4})";



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
            // Group 1: Full Date | Full Formal Date
            // Group 2: Month
            // Group 3: Day of the month
            // Group 4: Year
            // Group 5: Formal Month
            // Group 6: Formal Day of the month
            // Group 7: Formal Year
            // Group 8: Full Time of day
            // Group 9: Hour
            // Group 10: NOT USED
            // Group 11: Minutes
            // Group 12: am/pm
            Match dateTimeMatch = Regex.Match(input, $@"(?i)^({DateRegexString}|{FormalDateRegexString})?\s?({TimeOfDayRegexString})?$");

            // Return null if invalid date was inputted
            if (!dateTimeMatch.Success)
                return null;

            //Parse data
            string yearString = "", monthString = "", domString = "";

            if (dateTimeMatch.Groups[2].Value.Length > 0)
            {
                yearString = dateTimeMatch.Groups[4].ToString();
                monthString = dateTimeMatch.Groups[2].ToString().ToLower();
                domString = dateTimeMatch.Groups[3].ToString();
            }
            else if (dateTimeMatch.Groups[5].Value.Length > 0)
            {
                yearString = dateTimeMatch.Groups[7].ToString();
                monthString = dateTimeMatch.Groups[5].ToString().ToLower();
                domString = dateTimeMatch.Groups[6].ToString();
            }
            int year = 1;
            int month = 1;
            int dayOfMonth = 1;

            // only set year, month, and dayOfMonth if all 3 were matched
            if (yearString.Length > 0 && monthString.Length > 0 && domString.Length > 0)
            {
                int.TryParse(yearString, out year);
                //Use either number or bbreviated name
                if (!int.TryParse(monthString, out month))
                {
                    // Return null if month doesnt exist
                    if (!AbbreviatedMonths.Contains(monthString))
                        return null;
                    month = AbbreviatedMonths.IndexOf(monthString) + 1;
                }
                int.TryParse(domString, out dayOfMonth);
            }

            // Dont need to check if time was matched as 0 is in the range for all 3
            int.TryParse(dateTimeMatch.Groups[9].ToString(), out int hours);
            int.TryParse(dateTimeMatch.Groups[11].ToString(), out int minutes);
            string meridiem = dateTimeMatch.Groups[12].ToString().ToLower();

            // Enforce 12 hour format and 12 months
            if (hours > 12 || month > 12 || month < 1)
                return null;

            // Make 12 act as 0
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




        /// <summary>
        /// Converts a DateTime type to discords unix time stamp
        /// </summary>
        /// <param name="dateTime">dateTime obj</param>
        /// <param name="format">optional formater</param>
        /// <returns>discord unix time stamp</returns>
        public static string ToUnixTimeStamp(DateTime dateTime, string format = "")
        {
            if (format != "")
                format = $":{format}";

            return $"<t:{new DateTimeOffset(dateTime).ToUnixTimeSeconds()}{format}>";
        }



        /// <summary>
        /// Converts a epoch unix timestamp type to discords unix time stamp
        /// </summary>
        /// <param name="epochTimeStamp">epoch unix timestamp</param>
        /// <param name="format">optional formater</param>
        /// <returns>discord unix time stamp</returns>
        public static string ToUnixTimeStamp(long epochTimeStamp, string format = "")
        {
            if (format != "")
                format = $":{format}";

            return $"<t:{epochTimeStamp}{format}>";
        }



        /// <summary>
        /// Get the epoch unix timestamp for the start of the latest sunday
        /// </summary>
        /// <returns>epoch unix timestamp</returns>
        public static long GetSundayUnixTimeStamp(bool previousSunday, DateTime startTime = default)
        {
            if (startTime == default)
                startTime = DateTime.Now;

            // Calculate how many days to go back or forward to reach Sunday
            // DayOfWeek.Sunday == 0
            int daysToAdd = previousSunday
                            ? (startTime.DayOfWeek == 0 ? -7 : -(int)startTime.DayOfWeek)
                            : (7 - (int)startTime.DayOfWeek);


            DateTime sunday = startTime.AddDays(daysToAdd);

            return new DateTimeOffset(sunday.Date).ToUnixTimeSeconds();
        }



        /// <summary>
        /// Convert unix time stamp into date time
        /// </summary>
        /// <param name="unixTimeStamp">date in unix time stamp</param>
        /// <returns>Converted DateTime instance</returns>
        public static DateTime ToDateTime(long unixTimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).DateTime.ToLocalTime();
        }



        /// <summary>
        /// Add a console message with a time stamp and colours
        /// </summary>
        /// <param name="message">message to send</param>
        /// <param name="logLevel">Log level</param>
        public static void PrettyConsoleWriteLine(string message, LogLevel logLevel)
        {
            Dictionary<LogLevel, ConsoleColor> logLevelColors = new Dictionary<LogLevel, ConsoleColor>
            {
                { LogLevel.Info, ConsoleColor.Green },
                { LogLevel.Warning, ConsoleColor.Yellow },
                { LogLevel.Error, ConsoleColor.Red },
                { LogLevel.Debug, ConsoleColor.Cyan },
                { LogLevel.Trace, ConsoleColor.Magenta },
                { LogLevel.Critical, ConsoleColor.White },
                { LogLevel.Success, ConsoleColor.DarkGreen },
                { LogLevel.SystemMessage, ConsoleColor.Blue }
            };

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]");
            Console.ResetColor();
            Console.Write(" - ");
            Console.ForegroundColor = logLevelColors[logLevel];
            if (logLevel == LogLevel.Critical)
                Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine($"{Enum.GetName(typeof(LogLevel), logLevel)}: {message}");
            Console.ResetColor();
        }



        /// <summary>
        /// Log level for PrettyConsoleWriteLine()
        /// </summary>
        public enum LogLevel
        {
            Info,
            Warning,
            Error,
            Debug,
            Trace,
            Critical,
            Success,
            SystemMessage
        }
    }
}
