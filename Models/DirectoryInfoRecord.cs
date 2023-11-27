/* 2023/10/26 */

using System.ComponentModel;

namespace FileInfoTool.Models
{
    internal class DirectoryInfoRecord : FileSystemInfoRecord
    {
        public List<FileInfoRecord>? Files { get; set; }

        public List<DirectoryInfoRecord>? Directories { get; set; }

        [DefaultValue(false)]
        public bool GetFilesFailed { get; set; }

        [DefaultValue(false)]
        public bool GetDirectoriesFailed { get; set; }

        public override string GetRelativePath()
        {
            return base.GetRelativePath() + Path.DirectorySeparatorChar;
        }
    }
}
