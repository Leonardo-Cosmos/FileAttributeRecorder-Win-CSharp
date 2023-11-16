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

        private readonly bool restore;

        private readonly InfoProperty[] fileProperties;

        private readonly bool loadFileCreationTime;

        private readonly bool loadFileLastWriteTime;

        private readonly bool loadFileLastAccessTime;

        private readonly bool loadFileSize;

        private readonly InfoProperty[] dirProperties;

        private readonly bool loadDirCreationTime;

        private readonly bool loadDirLastWriteTime;

        private readonly bool loadDirLastAccessTime;

        public InfoLoader(string dirPath, string infoFilePath, bool restore,
            InfoProperty[]? fileProperties, InfoProperty[]? dirProperties)
        {
            this.dirPath = dirPath;
            this.infoFilePath = infoFilePath;
            this.restore = restore;

            if (fileProperties != null)
            {
                if (restore)
                {
                    this.fileProperties = InfoProperties.ValidRestoreFileProperties
                        .Intersect(fileProperties)
                        .ToArray();
                }
                else
                {
                    this.fileProperties = InfoProperties.ValidValidateFileProperties
                        .Intersect(fileProperties)
                        .ToArray();
                }
            }
            else
            {
                if (restore)
                {
                    this.fileProperties = new InfoProperty[]
                    {
                        InfoProperty.CreationTime,
                        InfoProperty.LastWriteTime,
                    };
                }
                else
                {
                    this.fileProperties = new InfoProperty[]
                    {
                        InfoProperty.CreationTime,
                        InfoProperty.LastWriteTime,
                        InfoProperty.Size,
                    };
                }
            }

            if (dirProperties != null)
            {
                this.dirProperties = InfoProperties.ValidDirProperties
                    .Intersect(dirProperties)
                    .ToArray();
            }
            else
            {
                this.dirProperties = new InfoProperty[] {
                    InfoProperty.CreationTime,
                    InfoProperty.LastWriteTime,
                };
            }

            loadFileCreationTime = this.fileProperties.Contains(InfoProperty.CreationTime);
            loadFileLastWriteTime = this.fileProperties.Contains(InfoProperty.LastWriteTime);
            loadFileLastAccessTime = this.fileProperties.Contains(InfoProperty.LastAccessTime);
            loadFileSize = this.fileProperties.Contains(InfoProperty.Size);

            loadDirCreationTime = this.dirProperties.Contains(InfoProperty.CreationTime);
            loadDirLastWriteTime = this.dirProperties.Contains(InfoProperty.LastWriteTime);
            loadDirLastAccessTime = this.dirProperties.Contains(InfoProperty.LastAccessTime);
        }

        public void Load(bool recursive)
        {
            Console.WriteLine($"Load file system info, directory: {dirPath}, info file: {infoFilePath}, recursive: {recursive}, restore: {restore}");
            var filePropertyNames = fileProperties.Select(property => property.ToNameString());
            Console.WriteLine($"File proerties: {string.Join(", ", filePropertyNames)}");
            var dirPropertyNames = dirProperties.Select(property => property.ToNameString());
            Console.WriteLine($"Directory properties: {string.Join(", ", dirPropertyNames)}");

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
                    var subDirInfoRecord = subDirInfoRecords
                        .Find(infoRecord => infoRecord.Name == subDirectory.Name);
                    if (subDirInfoRecord != null)
                    {
                        Load(subDirectory, subDirInfoRecord, recursive, restore);
                        loadedSubDirInfoRecords.Add(subDirInfoRecord);
                    }
                    else
                    {
                        PrintUnknownInfo(subDirectory);
                    }
                }

                var missedSubDirInfoRecords = subDirInfoRecords.Except(loadedSubDirInfoRecords);
                PrintMissedInfoRecords(directory, missedSubDirInfoRecords);
            }

            var fileInfoRecords = dirInfoRecord.Files ?? new List<FileInfoRecord>();
            var loadedFileInfoRecords = new List<FileInfoRecord>();
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
                var fileInfoRecord = fileInfoRecords
                    .Find(infoRecord => infoRecord.Name == file.Name);
                if (fileInfoRecord != null)
                {
                    LoadInfoRecord(file, fileInfoRecord, restore);
                    loadedFileInfoRecords.Add(fileInfoRecord);
                }
                else
                {
                    PrintUnknownInfo(file);
                }
            }

            var missedFileInfoRecords = fileInfoRecords.Except(loadedFileInfoRecords);
            PrintMissedInfoRecords(directory, missedFileInfoRecords);

            LoadInfoRecord(directory, dirInfoRecord, restore);
        }

        private void PrintUnknownInfo(FileSystemInfo info)
        {
            if (info is FileInfo file)
            {
                Console.WriteLine($"Unknown file {file.GetRelativePath(dirPath)}");
            }
            else if (info is DirectoryInfo directory)
            {
                Console.WriteLine($"Unknown file {directory.GetRelativePath(dirPath)}");
            }
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
            bool loadCreationTime = false;
            bool loadLastWriteTime = false;
            bool loadLastAccessTime = false;
            bool loadSize = false;
            if (info is FileInfo)
            {
                loadCreationTime = loadFileCreationTime;
                loadLastWriteTime = loadFileLastWriteTime;
                loadLastAccessTime = loadFileLastAccessTime;
                loadSize = loadFileSize;
            }
            else if (info is DirectoryInfo)
            {
                loadCreationTime = loadDirCreationTime;
                loadLastWriteTime = loadDirLastWriteTime;
                loadLastAccessTime = loadDirLastAccessTime;
            }

            string? changedCreationTimeUtc = null;
            var isCreationTimeChanged = loadCreationTime && IsDateTimeChanged(info.CreationTimeUtc, infoRecord.CreationTimeUtc);
            if (isCreationTimeChanged)
            {
                changedCreationTimeUtc = info.CreationTimeUtc.ToISOString();
                if (restore)
                {
                    info.CreationTimeUtc = DateTimeExtension.ParseISOString(infoRecord.CreationTimeUtc!);
                }
            }

            string? changedLastWriteTimeUtc = null;
            var isLastWriteTimeChanged = loadLastWriteTime && IsDateTimeChanged(info.LastWriteTimeUtc, infoRecord.LastWriteTimeUtc);
            if (isLastWriteTimeChanged)
            {
                changedLastWriteTimeUtc = info.LastWriteTimeUtc.ToISOString();
                if (restore)
                {
                    info.LastWriteTimeUtc = DateTimeExtension.ParseISOString(infoRecord.LastWriteTimeUtc!);
                }
            }

            string? changedLastAccessTimeUtc = null;
            var isLastAccessTimeChanged = loadLastAccessTime && IsDateTimeChanged(info.LastAccessTimeUtc, infoRecord.LastAccessTimeUtc);
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
            if (!restore && loadSize)
            {
                var file = info as FileInfo;
                var fileInfoRecord = infoRecord as FileInfoRecord;
                if (fileInfoRecord!.Size != null &&  file!.Length != fileInfoRecord!.Size)
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

        private static bool IsDateTimeChanged(DateTime dateTime, string? isoString)
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
