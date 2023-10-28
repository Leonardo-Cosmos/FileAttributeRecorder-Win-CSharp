/* 2023/10/26 */

namespace FileInfoTool.Models
{
    internal class InfoRecord
    {
        public const string RecordFileName = "Info.json";

        public required string RecordTimeUtc { get; set; }

        public long RecordTimeUtcTicks { get; set; }

        public required DirectoryInfoRecord Directory { get; set; }
    }
}
