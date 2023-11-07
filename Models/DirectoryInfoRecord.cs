/* 2023/10/26 */

using System.ComponentModel;
using System.Runtime.Serialization;

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
    }
}
