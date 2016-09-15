using System;
using System.Collections.Generic;

namespace LibSlyBroadcast
{
    static class Exts
    {
        public static DateTime ToEst(this DateTime dt)
        {
            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            return TimeZoneInfo.ConvertTime(dt, est);
        }

        public static string ToFormattedString(this DateTime dt) => dt.ToString("yyyy-MM-dd HH:MM:ss");

        public static string ToFormattedEstString(this DateTime dt) => dt.ToEst().ToFormattedString();

        public static string JoinWith(this IEnumerable<object> enumerable, string s) => string.Join(s, enumerable);
    }
}