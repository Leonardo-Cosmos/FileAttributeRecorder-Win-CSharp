/* 2023/10/27 */
using System.Globalization;

namespace FileInfoTool.Extensions
{
    internal static class DateTimeExtension
    {
        internal static string ToISOString(this DateTime dateTime)
        {
            return dateTime.ToString("O");
        }

        internal static bool ValidateISOString(this DateTime dateTime, string isoString)
        {
            return dateTime.ToISOString().Equals(isoString);
        }

        internal static DateTime ParseISOString(string isoString)
        {
            return DateTime.ParseExact(isoString, "O", CultureInfo.InvariantCulture);
        }
    }
}
