/* 2023/11/19 */
using System.Globalization;

namespace FileInfoTool.Extensions
{
    internal static class LongExtension
    {
        private const int kilo = 0x400;

        private static readonly string[] units = new string[] {
            "B",
            "KB",
            "MB",
            "GB",
        };

        private static readonly string lastUnit = "TB";

        internal static string ToByteString(this long value)
        {
            var number = (decimal)value;
            foreach (var unit in units)
            {
                if (number < kilo)
                {
                    return $"{ToByteDigitString(number)}{unit}";
                }
                else
                {
                    number /= kilo;
                }
            }
            return $"{ToByteDigitString(number)}{lastUnit}";
        }

        private static string ToByteDigitString(decimal value)
        {
            var str = value.ToString("####.##");
            if (str.Contains('.'))
            {
                var length = str.Length;
                var separatorIndex = str.IndexOf('.');
                return separatorIndex switch
                {
                    0 => $"0{str}",
                    1 => str,
                    2 => str[..(length < 4 ? length : 4)],
                    _ => str[..separatorIndex],
                };
            }
            else
            {
                return str;
            }
        }

        internal static string ToGroupString(this long value)
        {
            return value.ToString("#,#", CultureInfo.InvariantCulture);
        }

        internal static string ToByteDetailString(this long value)
        {
            return $"{value.ToByteString()} ({value.ToGroupString()} bytes)";
        }
    }
}
