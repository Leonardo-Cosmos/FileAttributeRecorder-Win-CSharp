/* 2023/10/26 */
using FileInfoTool.Extensions;
using FileInfoTool.Helpers;
using FileInfoTool.Models;

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

        private readonly bool loadFileHash;

        private readonly InfoProperty[] dirProperties;

        private readonly bool loadDirCreationTime;

        private readonly bool loadDirLastWriteTime;

        private readonly bool loadDirLastAccessTime;

        private int loadedFileCount;

        private int loadedDirectoryCount;

        private int changedFileCount;

        private int changedDirectoryCount;

        private int missingFileCount;

        private int missingDirectoryCount;

        private int unknownFileCount;

        private int unknownDirectoryCount;

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
            loadFileHash = this.fileProperties.Contains(InfoProperty.Hash);

            loadDirCreationTime = this.dirProperties.Contains(InfoProperty.CreationTime);
            loadDirLastWriteTime = this.dirProperties.Contains(InfoProperty.LastWriteTime);
            loadDirLastAccessTime = this.dirProperties.Contains(InfoProperty.LastAccessTime);
        }

        public void Load(bool recursive)
        {
            var filePropertyNames = fileProperties.Select(property => property.ToNameString());
            var dirPropertyNames = dirProperties.Select(property => property.ToNameString());

            Console.WriteLine($"""
                Load file system info
                    restore: {restore},
                    directory: {dirPath}
                    recursive: {recursive}
                    info file: {infoFilePath}
                    File proerties: {string.Join(", ", filePropertyNames)}
                    Directory properties: {string.Join(", ", dirPropertyNames)}

                """);

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

            var infoRecord = InfoSerializer.Deserialize(infoFilePath);
            if (infoRecord == null)
            {
                Console.WriteLine($"Invalid info file, path: {infoFilePath}");
                return;
            }

            loadedFileCount = 0;
            loadedDirectoryCount = 0;
            changedFileCount = 0;
            changedDirectoryCount = 0;
            missingFileCount = 0;
            missingDirectoryCount = 0;
            unknownFileCount = 0;
            unknownDirectoryCount = 0;
            Load(directory, infoRecord.Directory, recursive, restore);
            Console.WriteLine($"""
                Loaded
                    File: {loadedFileCount}
                    Directory: {loadedDirectoryCount}
                Changed
                    File: {changedFileCount}
                    Directory: {changedDirectoryCount}
                Missing
                    File: {missingFileCount}
                    Directory: {missingDirectoryCount}
                Unknown
                    File: {unknownFileCount}
                    Directory: {unknownDirectoryCount}
                """);
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

                var missingSubDirInfoRecords = subDirInfoRecords.Except(loadedSubDirInfoRecords);
                PrintMissingInfoRecords(directory, missingSubDirInfoRecords);
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

            var missingFileInfoRecords = fileInfoRecords.Except(loadedFileInfoRecords);
            PrintMissingInfoRecords(directory, missingFileInfoRecords);

            LoadInfoRecord(directory, dirInfoRecord, restore);
        }

        private void PrintUnknownInfo(FileSystemInfo info)
        {
            if (info is FileInfo file)
            {
                Console.WriteLine($"Unknown file {file.GetRelativePath(dirPath)}");
                unknownFileCount++;
            }
            else if (info is DirectoryInfo directory)
            {
                Console.WriteLine($"Unknown directory {directory.GetRelativePath(dirPath)}");
                unknownDirectoryCount++;
            }
        }

        private void PrintMissingInfoRecords(DirectoryInfo dirInfo, IEnumerable<FileSystemInfoRecord> infoRecords)
        {
            foreach (var infoRecord in infoRecords)
            {
                if (infoRecord is FileInfoRecord)
                {
                    var file = new FileInfo(Path.Combine(dirInfo.FullName, infoRecord.Name ?? "*"));
                    Console.WriteLine($"Missing file {file.GetRelativePath(dirPath)}");
                    missingFileCount++;
                }
                else if (infoRecord is DirectoryInfoRecord)
                {
                    var subDir = new DirectoryInfo(Path.Combine(dirInfo.FullName, infoRecord.Name ?? "*"));
                    Console.WriteLine($"Missing directory: {subDir.GetRelativePath(dirPath)}");
                    missingDirectoryCount++;
                }
            }
        }

        private void LoadInfoRecord(FileSystemInfo info, FileSystemInfoRecord infoRecord, bool restore)
        {
            bool loadCreationTime = false;
            bool loadLastWriteTime = false;
            bool loadLastAccessTime = false;
            bool loadSize = false;
            bool loadHash = false;
            if (info is FileInfo)
            {
                loadCreationTime = loadFileCreationTime;
                loadLastWriteTime = loadFileLastWriteTime;
                loadLastAccessTime = loadFileLastAccessTime;
                loadSize = loadFileSize;
                loadHash = loadFileHash;

                loadedFileCount++;
            }
            else if (info is DirectoryInfo)
            {
                loadCreationTime = loadDirCreationTime;
                loadLastWriteTime = loadDirLastWriteTime;
                loadLastAccessTime = loadDirLastAccessTime;

                loadedDirectoryCount++;
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
                FileInfo file = (info as FileInfo)!;
                FileInfoRecord fileInfoRecord = (infoRecord as FileInfoRecord)!;
                if (fileInfoRecord.Size != null && file.Length != fileInfoRecord.Size)
                {
                    isFileSizeChanged = true;
                    changedFileSize = file.Length;
                }
            }

            string? changedFileSHA512 = null;
            bool isFileHashChanged = false;
            if (!restore && loadHash)
            {
                FileInfo file = (info as FileInfo)!;
                FileInfoRecord fileInfoRecord = (infoRecord as FileInfoRecord)!;

                if (fileInfoRecord.SHA512 != null)
                {
                    Console.WriteLine($"Hash {file.GetRelativePath(dirPath)}");
                    ProgressPrinter progressPrinter = new("{0} ({1} / {2}), {3}/s");
                    var sha512 = HashComputer.ComputeHash(file.FullName, hashProgress =>
                    {
                        progressPrinter.Update(hashProgress.Percentage,
                            hashProgress.TotalUpdatedLength.ToByteString(),
                            hashProgress.TotalLength.ToByteString(),
                            hashProgress.LengthPerSecond);
                    });
                    progressPrinter.End();

                    if (fileInfoRecord.SHA512 != sha512)
                    {
                        isFileHashChanged = true;
                        changedFileSHA512 = sha512;
                    }
                }
            }

            var isChanged = isCreationTimeChanged || isLastWriteTimeChanged || isLastAccessTimeChanged
                || isFileSizeChanged || isFileHashChanged;

            PrintLoadedInfoRecord(info, infoRecord, restore, isChanged,
                changedCreationTimeUtc,
                changedLastWriteTimeUtc,
                changedLastAccessTimeUtc,
                changedFileSize,
                changedFileSHA512);
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

        private void PrintLoadedInfoRecord(FileSystemInfo info, FileSystemInfoRecord infoRecord, bool restore, bool isChanged,
            string? changedCreationTimeUtc,
            string? changedLastWriteTimeUtc,
            string? changedLastAccessTimeUtc,
            long? changedFileSize,
            string? changedFileSHA512)
        {
            if (isChanged)
            {
                if (restore)
                {
                    Console.Write("Restored");
                }
                else
                {
                    Console.Write("Detected");
                }
            }
            else
            {
                Console.Write("Same");
            }

            if (infoRecord is FileInfoRecord)
            {
                Console.Write(" file");
                if (isChanged)
                {
                    changedFileCount++;
                }
            }
            else if (infoRecord is DirectoryInfoRecord)
            {
                Console.Write(" directory");
                if (isChanged)
                {
                    changedDirectoryCount++;
                }
            }

            Console.WriteLine($" {info.GetRelativePath(dirPath)}");
            if (changedCreationTimeUtc != null)
            {
                Console.WriteLine($"  date created: {infoRecord.CreationTimeUtc} -> {changedCreationTimeUtc}");
            }
            if (changedLastWriteTimeUtc != null)
            {
                Console.WriteLine($"  date modified: {infoRecord.LastWriteTimeUtc} -> {changedLastWriteTimeUtc}");
            }
            if (changedLastAccessTimeUtc != null)
            {
                Console.WriteLine($"  date accessed: {infoRecord.LastAccessTimeUtc} -> {changedLastAccessTimeUtc}");
            }
            if (infoRecord is FileInfoRecord fileInfoRecord)
            {
                if (changedFileSize != null)
                {
                    Console.WriteLine($"  size: {fileInfoRecord!.Size?.ToByteDetailString()} -> {changedFileSize?.ToByteDetailString()}");
                }
                if (changedFileSHA512 != null)
                {
                    Console.WriteLine($"  SHA512: {fileInfoRecord.SHA512} -> {changedFileSHA512}");
                }
            }
        }
    }
}
