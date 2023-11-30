/* 2020/3/13 */

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace FileInfoTool.Models
{
    internal class FileInfoRecord : FileSystemInfoRecord
    {
        public long? Size { get; set; }

        [JsonPropertyName("sha512")]
        public string? SHA512 { get; set; }

        [DefaultValue(false)]
        public bool ComputeHashFailed { get; set; }
    }
}
