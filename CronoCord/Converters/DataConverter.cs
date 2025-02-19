using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CronoCord.Converters
{
    public class DataConverter
    {
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
    }
}
