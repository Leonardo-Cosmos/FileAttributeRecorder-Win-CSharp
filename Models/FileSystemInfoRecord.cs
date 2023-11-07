/* 2023/10/27 */

namespace FileInfoTool.Models
{
    internal abstract class FileSystemInfoRecord
    {
        public string? Name { get; set; }

        public string? CreationTimeUtc { get; set; }

        public long? CreationTimeUtcTicks { get; set; }

        public string? LastWriteTimeUtc { get; set; }

        public long? LastWriteTimeUtcTicks { get; set; }

        public string? LastAccessTimeUtc { get; set; }

        public long? LastAccessTimeUtcTicks { get; set; }
    }
}
