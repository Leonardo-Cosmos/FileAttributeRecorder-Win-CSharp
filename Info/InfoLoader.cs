/* 2023/10/26 */
using FileInfoTool.Extensions;
using FileInfoTool.Models;
using Newtonsoft.Json;

namespace FileInfoTool.Info
{
    internal class InfoLoader
    {
        private readonly string dirPath;

        private readonly string infoFilePath;

        public InfoLoader(string dirPath, string infoFilePath)
        {
            this.dirPath = dirPath;
            this.infoFilePath = infoFilePath;
        }

        public void Load(bool recursive, bool restore)
        {
            Console.WriteLine($"Load file system info, directory: {dirPath}, info file: {infoFilePath}, recursive: {recursive}, restore: {restore}");

            var directory = new DirectoryInfo(dirPath);
            if (!directory.Exists)
            {
                Console.WriteLine($"Directory doesn't exist, path: {dirPath}");
                return;
            }

            if (!File.Exists(infoFilePath))
            {
                Console.WriteLine($"Info file doesn't exist, path: {infoFilePath}");
                return;
            }

            var json = File.ReadAllText(infoFilePath);
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
            if (recursive)
            {
                var subDirInfoRecords = dirInfoRecord.Directories ?? new List<DirectoryInfoRecord>();
                var loadedSubDirInfoRecords = new List<DirectoryInfoRecord>();
                foreach (var subDirectory in directory.GetDirectories())
                {
                    var subDirInfoRecord = subDirInfoRecords
                        .Find(infoRecord => infoRecord.Name == subDirectory.Name);
                    if (subDirInfoRecord != null)
                    {
                        LoadInfoRecord(subDirectory, subDirInfoRecord, restore);
                        loadedSubDirInfoRecords.Add(subDirInfoRecord);
                    }
                }

                var missedSubDirInfoRecords = subDirInfoRecords.Except(loadedSubDirInfoRecords);
                PrintMissedInfoRecords(directory, missedSubDirInfoRecords);
            }

            var fileInfoRecords = dirInfoRecord.Files ?? new List<FileInfoRecord>();
            var loadedFileInfoRecords = new List<FileInfoRecord>();
            foreach (var file in directory.GetFiles())
            {
                var fileInfoRecord = fileInfoRecords
                    .Find(infoRecord => infoRecord.Name == file.Name);
                if (fileInfoRecord != null)
                {
                    LoadInfoRecord(file, fileInfoRecord, restore);
                    loadedFileInfoRecords.Add(fileInfoRecord);
                }
            }

            var missedFileInfoRecords = fileInfoRecords.Except(loadedFileInfoRecords);
            PrintMissedInfoRecords(directory, missedFileInfoRecords);

            LoadInfoRecord(directory, dirInfoRecord, restore);
        }

        private void PrintMissedInfoRecords(DirectoryInfo dirInfo, IEnumerable<FileSystemInfoRecord> infoRecords)
        {
            foreach (var infoRecord in infoRecords)
            {
                if (infoRecord is FileInfoRecord)
                {
                    var file = new FileInfo(Path.Combine(dirInfo.FullName, infoRecord.Name ?? "*"));
                    Console.WriteLine($"Missed file {file.GetRelativePath(dirPath)}");
                }
                else if (infoRecord is DirectoryInfoRecord)
                {
                    var subDir = new DirectoryInfo(Path.Combine(dirInfo.FullName, infoRecord.Name ?? "*"));
                    Console.WriteLine($"Missed directory: {subDir.GetRelativePath(dirPath)}");
                }
            }
        }

        private void LoadInfoRecord(FileSystemInfo info, FileSystemInfoRecord infoRecord, bool restore)
        {
            string? changedCreationTimeUtc = null;
            var isCreationTimeChanged = ValidateISOString(info.CreationTimeUtc, infoRecord.CreationTimeUtc);
            if (isCreationTimeChanged)
            {
                changedCreationTimeUtc = info.CreationTimeUtc.ToISOString();
                if (restore)
                {
                    info.CreationTimeUtc = DateTimeExtension.ParseISOString(infoRecord.CreationTimeUtc!);
                }
            }

            string? changedLastWriteTimeUtc = null;
            var isLastWriteTimeChanged = ValidateISOString(info.LastWriteTimeUtc, infoRecord.LastWriteTimeUtc);
            if (isLastWriteTimeChanged)
            {
                changedLastWriteTimeUtc = info.LastWriteTimeUtc.ToISOString();
                if (restore)
                {
                    info.LastWriteTimeUtc = DateTimeExtension.ParseISOString(infoRecord.LastWriteTimeUtc!);
                }
            }

            string? changedLastAccessTimeUtc = null;
            var isLastAccessTimeChanged = ValidateISOString(info.LastAccessTimeUtc, infoRecord.LastAccessTimeUtc);
            if (isLastAccessTimeChanged)
            {
                changedLastAccessTimeUtc = info.LastAccessTimeUtc.ToISOString();
                if (restore)
                {
                    info.LastAccessTimeUtc = DateTimeExtension.ParseISOString(infoRecord.LastAccessTimeUtc!);
                }
            }

            long? changedFileSize = null;
            bool isFileSizeChanged = false;
            if (!restore && info is FileInfo file)
            {
                var fileInfoRecord = infoRecord as FileInfoRecord;
                if (file.Length != fileInfoRecord!.Size)
                {
                    isFileSizeChanged = true;
                    changedFileSize = file.Length;
                }
            }

            if (isCreationTimeChanged || isLastWriteTimeChanged || isLastAccessTimeChanged
                || isFileSizeChanged)
            {
                PrintLoadedInfoRecord(info, infoRecord, restore,
                    changedCreationTimeUtc,
                    changedLastWriteTimeUtc,
                    changedLastAccessTimeUtc,
                    changedFileSize);
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
                return !dateTime.ToISOString().Equals(isoString);
            }
        }

        private void PrintLoadedInfoRecord(FileSystemInfo info, FileSystemInfoRecord infoRecord, bool restore,
            string? changedCreationTimeUtc,
            string? changedLastWriteTimeUtc,
            string? changedLastAccessTimeUtc,
            long? changedFileSize)
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

            Console.WriteLine($" {info.GetRelativePath(dirPath)}");
            if (changedCreationTimeUtc != null)
            {
                Console.WriteLine($"  creation time: {infoRecord.CreationTimeUtc} -> {changedCreationTimeUtc}");
            }
            if (changedLastWriteTimeUtc != null)
            {
                Console.WriteLine($"  last write time: {infoRecord.LastWriteTimeUtc} -> {changedLastWriteTimeUtc},");
            }
            if (changedLastAccessTimeUtc != null)
            {
                Console.WriteLine($"  last access time: {infoRecord.LastAccessTimeUtc} -> {changedLastAccessTimeUtc}");
            }
            if (changedFileSize != null)
            {
                var fileInfoRecord = infoRecord as FileInfoRecord;
                Console.WriteLine($"  size: {fileInfoRecord!.Size} -> {changedFileSize}");
            }
        }
    }
}
