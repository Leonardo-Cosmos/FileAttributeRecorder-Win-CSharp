/* 2023/10/26 */
using FileInfoTool.Extensions;
using FileInfoTool.Models;
using Newtonsoft.Json;

namespace FileInfoTool.Info
{
    internal class InfoLoader
    {
        public void Load(string dirPath, bool recursive, bool restore)
        {
            Console.WriteLine($"Load directory: path: {dirPath}, recursive: {recursive}, restore: {restore}");

            var directory = new DirectoryInfo(dirPath);
            if (!directory.Exists)
            {
                Console.WriteLine($"Directory doesn't exist, path: {dirPath}");
                return;
            }

            var json = File.ReadAllText(Path.Combine(dirPath, InfoRecord.RecordFileName));
            var infoRecord = JsonConvert.DeserializeObject<InfoRecord>(json);
            if (infoRecord == null)
            {
                Console.WriteLine("Invalid info file");
                return;
            }

            Load(directory, infoRecord.Directory, recursive, restore);
        }

        private void Load(DirectoryInfo directory, DirectoryInfoRecord dirInfoRecord,
            bool recursive, bool restore)
        {
            LoadInfoRecord(directory, dirInfoRecord, restore);

            var fileInfoRecords = dirInfoRecord.Files ?? new List<FileSystemInfoRecord>();
            foreach (var file in directory.GetFiles())
            {
                var fileInfoRecord = fileInfoRecords
                    .Find(infoRecord => infoRecord.Name == file.Name);
                if (fileInfoRecord != null)
                {
                    LoadInfoRecord(file, fileInfoRecord, restore);
                }
            }

            if (recursive)
            {
                var subDirInfoRecords = dirInfoRecord.Directories ?? new List<DirectoryInfoRecord>();
                foreach (var subDirectory in directory.GetDirectories())
                {
                    var subDirInfoRecord = subDirInfoRecords
                        .Find(infoRecord => infoRecord.Name == subDirectory.Name);
                    if (subDirInfoRecord != null)
                    {
                        LoadInfoRecord(subDirectory, subDirInfoRecord, restore);
                    }
                }
            }
        }

        private static void LoadInfoRecord(FileSystemInfo info, FileSystemInfoRecord infoRecord, bool restore)
        {
            var isCreationTimeChanged = ValidateISOString(info.CreationTimeUtc, infoRecord.CreationTimeUtc);
            if (isCreationTimeChanged && restore)
            {
                info.CreationTimeUtc = DateTimeExtension.ParseISOString(infoRecord.CreationTimeUtc!);
            }

            var isLastWriteTimeChanged = ValidateISOString(info.LastWriteTimeUtc, infoRecord.LastWriteTimeUtc);
            if (isLastWriteTimeChanged && restore)
            {
                info.LastWriteTimeUtc = DateTimeExtension.ParseISOString(infoRecord.LastWriteTimeUtc!);
            }

            var isLastAccessTimeChanged = ValidateISOString(info.LastAccessTimeUtc, infoRecord.LastAccessTimeUtc);
            if (isLastAccessTimeChanged && restore)
            {
                info.LastAccessTimeUtc = DateTimeExtension.ParseISOString(infoRecord.LastAccessTimeUtc!);
            }

            if (isCreationTimeChanged || isLastWriteTimeChanged || isLastAccessTimeChanged)
            {
                PrintLoadedInfoRecord(infoRecord, restore,
                    isCreationTimeChanged, isLastWriteTimeChanged, isLastAccessTimeChanged);
            }
        }

        private static bool ValidateISOString(DateTime dateTime, string? isoString)
        {
            if (isoString == null)
            {
                return false;
            }
            else
            {
                return dateTime.ValidateISOString(isoString);
            }
        }

        private static void PrintLoadedInfoRecord(FileSystemInfoRecord infoRecord, bool restore,
            bool isCreationTimeChanged, bool isLastWriteTimeChanged, bool isLastAccessTimeChanged)
        {
            if (restore)
            {
                Console.Write("Restored");
            }
            else
            {
                Console.Write("Detected");
            }

            if (infoRecord is FileInfoRecord)
            {
                Console.Write(" file");
            }
            else if (infoRecord is DirectoryInfoRecord)
            {
                Console.Write(" directory");
            }

            Console.WriteLine($" name: {infoRecord.Name}");
            if (isCreationTimeChanged)
            {
                Console.WriteLine($"  creation time: {infoRecord.CreationTimeUtc}");
            }
            if (isLastWriteTimeChanged)
            {
                Console.WriteLine($"  last write time: {infoRecord.LastWriteTimeUtc},");
            }
            if (isLastAccessTimeChanged)
            {
                Console.WriteLine($"  last access time: {infoRecord.LastAccessTimeUtc}");
            }
        }
    }
}
