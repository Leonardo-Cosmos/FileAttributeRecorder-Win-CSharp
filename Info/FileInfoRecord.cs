/* 2020/3/13 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAttributeRecorder.Info
{
    class FileInfoRecord
    {
        public required string FileName { get; set; }

        public required string CreationTimeUtc { get; set; }

        public long CreationTimeUtcTicks { get; set; }

        public required string LastWriteTimeUtc { get; set; }

        public long LastWriteTimeUtcTicks { get; set; }

        public required string LastAccessTimeUtc { get; set; }

        public long LastAccessTimeUtcTicks { get; set; }

    }
}
