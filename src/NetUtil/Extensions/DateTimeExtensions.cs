using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetUtil
{
    internal static class DateTimeExtensions
    {
        public static string ToUtcIso8601String(this DateTime dateTime)
        {
            // To Universal Time assumes local if DateTime.Kind is unspecified. If already Utc then time is not converted.
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ");
        }

        public static string ToIso8601String(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return ToUtcIso8601String(dateTime);
            else
                return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
        }
    }
}
