/* 2023/10/26 */

namespace FileInfoTool.Models
{
    internal class DirectoryInfoRecord : FileSystemInfoRecord
    {
        public List<FileSystemInfoRecord>? Files { get; set; }

        public List<DirectoryInfoRecord>? Directories { get; set; }
    }
}
