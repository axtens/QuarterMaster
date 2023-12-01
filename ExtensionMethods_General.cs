using System;
using System.Text.RegularExpressions;

namespace QuarterMaster
{
    public static class ExtensionMethods
    {
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date - epoch).TotalSeconds);
        }

        public static string Between(this string source, string left, string right)
        {
            return Regex.Match(
                    source,
                    string.Format("{0}(.*?){1}", left, right))
                .Groups[1].Value;
        }
    }
}
