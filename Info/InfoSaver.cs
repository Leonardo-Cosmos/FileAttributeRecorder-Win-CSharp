/* 2023/10/27 */
using FileInfoTool.Extensions;
using FileInfoTool.Models;
using Newtonsoft.Json;

namespace FileInfoTool.Info
{
    internal class InfoSaver
    {
        private readonly string dirPath;

        private readonly string infoFilePath;

        public InfoSaver(string dirPath, string infoFilePath)
        {
            this.dirPath = dirPath;
            this.infoFilePath = infoFilePath;
        }

        public void Save(bool recursive)
        {
            Console.WriteLine($"Save file system info, directory: {dirPath}, info file: {infoFilePath}, recursive: {recursive}");
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

            string json = JsonConvert.SerializeObject(infoRecord, Formatting.Indented,
                new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
            File.WriteAllText(infoFilePath, json);
        }

        private DirectoryInfoRecord Save(DirectoryInfo directory, bool recursive)
        {
            var dirInfoRecord = SaveInfoRecord<DirectoryInfoRecord>(directory);

            var fileInfoRecords = new List<FileInfoRecord>();
            FileInfo[] files;
            try
            {
                files = directory.GetFiles();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                dirInfoRecord.GetFilesFailed = true;
                files = Array.Empty<FileInfo>();
            }
            foreach (var file in files)
            {
                var fileInfoRecord = SaveInfoRecord<FileInfoRecord>(file);
                fileInfoRecords.Add(fileInfoRecord);
            }
            dirInfoRecord.Files = fileInfoRecords;

            if (recursive)
            {
                var subDirInfoRecords = new List<DirectoryInfoRecord>();
                DirectoryInfo[] subDirectories;
                try
                {
                    subDirectories = directory.GetDirectories();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    dirInfoRecord.GetDirectoriesFailed = true;
                    subDirectories = Array.Empty<DirectoryInfo>();
                }
                foreach (var subDirectory in subDirectories)
                {
                    var subDirInfoRecord = Save(subDirectory, recursive: true);
                    subDirInfoRecords.Add(subDirInfoRecord);
                }
                dirInfoRecord.Directories = subDirInfoRecords;
            }

            return dirInfoRecord;
        }

        private InfoRecord SaveInfoRecord<InfoRecord>(FileSystemInfo info)
            where InfoRecord : FileSystemInfoRecord, new()
        {
            var infoRecord = new InfoRecord()
            {
                Name = info.Name,
                CreationTimeUtc = info.CreationTimeUtc.ToISOString(),
                CreationTimeUtcTicks = info.CreationTimeUtc.Ticks,
                LastWriteTimeUtc = info.LastWriteTimeUtc.ToISOString(),
                LastWriteTimeUtcTicks = info.LastWriteTimeUtc.Ticks,
                LastAccessTimeUtc = info.LastAccessTimeUtc.ToISOString(),
                LastAccessTimeUtcTicks = info.LastAccessTimeUtc.Ticks,
            };

            if (info is FileInfo file)
            {
                var fileInfoRecord = infoRecord as FileInfoRecord;
                fileInfoRecord!.Size = file.Length;
            }

            PrintSavedInfoRecord(info, infoRecord);
            return infoRecord;
        }

        private void PrintSavedInfoRecord(FileSystemInfo info, FileSystemInfoRecord infoRecord)
        {
            if (infoRecord is FileInfoRecord)
            {
                Console.Write("Saved file");
            }
            else if (infoRecord is DirectoryInfoRecord)
            {
                Console.Write("Saved directory");
            }
            Console.WriteLine($" {info.GetRelativePath(dirPath)}");
            Console.WriteLine($"  creation time: {infoRecord.CreationTimeUtc}");
            Console.WriteLine($"  last write time: {infoRecord.LastWriteTimeUtc},");
            Console.WriteLine($"  last access time: {infoRecord.LastAccessTimeUtc}");
            if (infoRecord is FileInfoRecord fileInfoRecord)
            {
                Console.WriteLine($"  size: {fileInfoRecord.Size}");
            }
        }
    }
}
