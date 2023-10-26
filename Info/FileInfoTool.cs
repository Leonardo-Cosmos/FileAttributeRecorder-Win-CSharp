/* 2020/3/13 */
using FileInfoTool.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInfoTool.Info
{
    class FileInfoTool
    {
        private const string RecordFileName = "Info.json";

        private readonly string _dirPath;

        public FileInfoTool(string dirPath)
        {
            _dirPath = dirPath;
        }

        public void SaveFileDate()
        {
            var dir = new DirectoryInfo(_dirPath);

            var fileInfoRecords = new List<FileInfoRecord>();
            foreach (var file in dir.GetFiles())
            {
                if (RecordFileName == file.Name)
                {
                    continue;
                }

                var fileInfoRecord = new FileInfoRecord()
                {
                    FileName = file.Name,
                    CreationTimeUtc = file.CreationTimeUtc.ToString(),
                    CreationTimeUtcTicks = file.CreationTimeUtc.Ticks,
                    LastWriteTimeUtc = file.LastWriteTimeUtc.ToString(),
                    LastWriteTimeUtcTicks = file.LastWriteTimeUtc.Ticks,
                    LastAccessTimeUtc = file.LastAccessTimeUtc.ToString(),
                    LastAccessTimeUtcTicks = file.LastAccessTimeUtc.Ticks,
                };
                fileInfoRecords.Add(fileInfoRecord);
            }

            string json = JsonConvert.SerializeObject(fileInfoRecords, Formatting.Indented);
            File.WriteAllText(Path.Combine(_dirPath, RecordFileName), json);
        }

        public void RestoreFileDate()
        {
            var json = File.ReadAllText(Path.Combine(_dirPath, RecordFileName));
            var fileInfoRecords = JsonConvert.DeserializeObject<List<FileInfoRecord>>(json);
            if (fileInfoRecords == null)
            {
                Console.WriteLine("Invalid info file");
                return;
            }
            foreach (var fileInfoRecord in fileInfoRecords)
            {
                var file = new FileInfo(Path.Combine(_dirPath, fileInfoRecord.FileName));
                if (file.Exists)
                {
                    file.CreationTimeUtc = new DateTime(fileInfoRecord.CreationTimeUtcTicks);
                    file.LastWriteTimeUtc = new DateTime(fileInfoRecord.LastWriteTimeUtcTicks);
                    file.LastAccessTimeUtc = new DateTime(fileInfoRecord.LastAccessTimeUtcTicks);
                }
            }
        }

    }
}
