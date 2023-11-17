/* 2023/10/26 */

using FileInfoTool.Extensions;

namespace FileInfoTool.Models
{
    internal class InfoRecord
    {
        public const string DefaultFileNameFormat = "{0}_Info.json";

        public required string RecordTimeUtc { get; set; }

        public long RecordTimeUtcTicks { get; set; }

        public required DirectoryInfoRecord Directory { get; set; }

        /// <summary>
        /// Create an instance with current time.
        /// </summary>
        /// <param name="dirInfoRecord"></param>
        /// <returns></returns>
        public static InfoRecord Create(DirectoryInfoRecord dirInfoRecord)
        {
            var currentDateTime = DateTime.UtcNow;
            return new InfoRecord()
            {
                Directory = dirInfoRecord,
                RecordTimeUtc = currentDateTime.ToISOString(),
                RecordTimeUtcTicks = currentDateTime.Ticks,
            };
        }

        /// <summary>
        /// Update an instance with current time.
        /// </summary>
        /// <param name="infoRecord"></param>
        public static void Update(InfoRecord infoRecord)
        {
            var currentDateTime = DateTime.UtcNow;
            infoRecord.RecordTimeUtc = currentDateTime.ToISOString();
            infoRecord.RecordTimeUtcTicks = currentDateTime.Ticks;
        }
    }
}
