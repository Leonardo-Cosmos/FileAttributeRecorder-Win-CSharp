/* 2023/10/27 */
using FileInfoTool.Extensions;
using FileInfoTool.Models;
using Newtonsoft.Json;

namespace FileInfoTool.Info
{
    internal class InfoSaver
    {
        public void Save(string dirPath, bool recursive)
        {
            Console.WriteLine($"Save file system info of directory, path: {dirPath}, recursive: {recursive}");
            var directory = new DirectoryInfo(dirPath);
            if (!directory.Exists)
            {
                Console.WriteLine($"Directory doesn't exist, path: {dirPath}");
                return;
            }

            var dirInfoRecord = Save(directory, recursive);

            var currentDateTime = DateTime.UtcNow;
            var infoRecord = new InfoRecord()
            {
                Directory = dirInfoRecord,
                RecordTimeUtc = currentDateTime.ToISOString(),
                RecordTimeUtcTicks = currentDateTime.Ticks,
            };

            string json = JsonConvert.SerializeObject(infoRecord, Formatting.Indented);
            File.WriteAllText(Path.Combine(dirPath, InfoRecord.RecordFileName), json);
        }

        private DirectoryInfoRecord Save(DirectoryInfo directory, bool recursive)
        {
            var dirInfoRecord = SaveInfoRecord<DirectoryInfoRecord>(directory);
            PrintSavedInfoRecord(dirInfoRecord);

            var fileInfoRecords = new List<FileSystemInfoRecord>();
            foreach (var file in directory.GetFiles())
            {
                if (InfoRecord.RecordFileName == file.Name)
                {
                    continue;
                }

                var fileInfoRecord = SaveInfoRecord<FileInfoRecord>(file);
                PrintSavedInfoRecord(fileInfoRecord);
                fileInfoRecords.Add(fileInfoRecord);
            }
            dirInfoRecord.Files = fileInfoRecords;

            if (recursive)
            {
                var subDirInfoRecords = new List<DirectoryInfoRecord>();
                foreach (var subDirectory in directory.GetDirectories())
                {
                    var subDirInfoRecord = Save(subDirectory, recursive: true);
                    subDirInfoRecords.Add(subDirInfoRecord);
                }
                dirInfoRecord.Directories = subDirInfoRecords;
            }

            return dirInfoRecord;
        }

        private static InfoRecord SaveInfoRecord<InfoRecord>(FileSystemInfo info)
            where InfoRecord : FileSystemInfoRecord, new()
        {
            return new InfoRecord()
            {
                Name = info.Name,
                CreationTimeUtc = info.CreationTimeUtc.ToISOString(),
                CreationTimeUtcTicks = info.CreationTimeUtc.Ticks,
                LastWriteTimeUtc = info.LastWriteTimeUtc.ToISOString(),
                LastWriteTimeUtcTicks = info.LastWriteTimeUtc.Ticks,
                LastAccessTimeUtc = info.LastAccessTimeUtc.ToISOString(),
                LastAccessTimeUtcTicks = info.LastAccessTimeUtc.Ticks,
            };
        }

        private static void PrintSavedInfoRecord(FileSystemInfoRecord infoRecord)
        {
            if (infoRecord is FileInfoRecord)
            {
                Console.Write("Saved file");
            }
            else if (infoRecord is DirectoryInfoRecord)
            {
                Console.Write("Saved directory");
            }
            Console.WriteLine($" name: {infoRecord.Name}");
            Console.WriteLine($"  creation time: {infoRecord.CreationTimeUtc}");
            Console.WriteLine($"  last write time: {infoRecord.LastWriteTimeUtc},");
            Console.WriteLine($"  last access time: {infoRecord.LastAccessTimeUtc}");
        }
    }
}
