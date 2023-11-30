/* 2023/10/27 */
using FileInfoTool.Extensions;
using FileInfoTool.Helpers;
using FileInfoTool.Models;

namespace FileInfoTool.Info
{
    internal class InfoSaver
    {
        private readonly string dirPath;

        private readonly string infoFilePath;

        private readonly InfoProperty[] fileProperties;

        private readonly bool saveFileCreationTime;

        private readonly bool saveFileLastWriteTime;

        private readonly bool saveFileLastAccessTime;

        private readonly bool saveFileSize;

        private readonly bool saveFileHash;

        private readonly InfoProperty[] dirProperties;

        private readonly bool saveDirCreationTime;

        private readonly bool saveDirLastWriteTime;

        private readonly bool saveDirLastAccessTime;

        private int savedFileCount;

        private int savedDirectoryCount;

        public InfoSaver(string dirPath, string infoFilePath,
            InfoProperty[]? fileProperties, InfoProperty[]? dirProperties)
        {
            this.dirPath = dirPath;
            this.infoFilePath = infoFilePath;

            if (fileProperties != null)
            {
                this.fileProperties = InfoProperties.ValidSaveFileProperties
                        .Intersect(fileProperties)
                        .ToArray();
            }
            else
            {
                this.fileProperties = new InfoProperty[]
                    {
                        InfoProperty.CreationTime,
                        InfoProperty.LastWriteTime,
                        InfoProperty.LastAccessTime,
                        InfoProperty.Size,
                    };
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
                    InfoProperty.LastAccessTime,
                };
            }

            saveFileCreationTime = this.fileProperties.Contains(InfoProperty.CreationTime);
            saveFileLastWriteTime = this.fileProperties.Contains(InfoProperty.LastWriteTime);
            saveFileLastAccessTime = this.fileProperties.Contains(InfoProperty.LastAccessTime);
            saveFileSize = this.fileProperties.Contains(InfoProperty.Size);
            saveFileHash = this.fileProperties.Contains(InfoProperty.Hash);

            saveDirCreationTime = this.dirProperties.Contains(InfoProperty.CreationTime);
            saveDirLastWriteTime = this.dirProperties.Contains(InfoProperty.LastWriteTime);
            saveDirLastAccessTime = this.dirProperties.Contains(InfoProperty.LastAccessTime);
        }

        public void Save(bool recursive, bool overwrite)
        {
            var filePropertyNames = fileProperties.Select(property => property.ToNameString());
            var dirPropertyNames = dirProperties.Select(property => property.ToNameString());

            Console.WriteLine($"""
                Save file system info
                    directory: {dirPath}
                    recursive: {recursive}
                    info file: {infoFilePath}
                    overwrite: {overwrite}
                    File proerties: {string.Join(", ", filePropertyNames)}
                    Directory properties: {string.Join(", ", dirPropertyNames)}

                """);

            var directory = new DirectoryInfo(dirPath);
            if (!directory.Exists)
            {
                Console.WriteLine($"Directory doesn't exist, path: {dirPath}");
                return;
            }

            if (File.Exists(infoFilePath) && !overwrite)
            {
                Console.WriteLine($"Info file exists already, path: {infoFilePath}");
                return;
            }

            savedFileCount = 0;
            savedDirectoryCount = 0;
            var dirInfoRecord = Save(directory, recursive);
            Console.WriteLine($"""
                Saved
                    File: {savedFileCount}
                    Directory: {savedDirectoryCount}
                """);

            var infoRecord = InfoRecord.Create(dirInfoRecord);

            InfoSerializer.Serialize(infoRecord, infoFilePath);
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
            fileInfoRecords.Sort(FileSystemInfoRecord.CompareByName);
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
                    var subDirInfoRecord = Save(subDirectory, recursive);
                    subDirInfoRecords.Add(subDirInfoRecord);
                }
                subDirInfoRecords.Sort(FileSystemInfoRecord.CompareByName);
                dirInfoRecord.Directories = subDirInfoRecords;
            }

            return dirInfoRecord;
        }

        private InfoRecord SaveInfoRecord<InfoRecord>(FileSystemInfo info)
            where InfoRecord : FileSystemInfoRecord, new()
        {
            bool saveCreationTime = false;
            bool saveLastWriteTime = false;
            bool saveLastAccessTime = false;
            bool saveSize = false;
            bool saveHash = false;
            if (info is FileInfo)
            {
                savedFileCount++;

                saveCreationTime = saveFileCreationTime;
                saveLastWriteTime = saveFileLastWriteTime;
                saveLastAccessTime = saveFileLastAccessTime;
                saveSize = saveFileSize;
                saveHash = saveFileHash;
            }
            else if (info is DirectoryInfo)
            {
                savedDirectoryCount++;

                saveCreationTime = saveDirCreationTime;
                saveLastWriteTime = saveDirLastWriteTime;
                saveLastAccessTime = saveDirLastAccessTime;
            }

            var infoRecord = new InfoRecord()
            {
                Name = info.Name,
            };

            if (saveCreationTime)
            {
                infoRecord.CreationTimeUtc = info.CreationTimeUtc.ToISOString();
                infoRecord.CreationTimeUtcTicks = info.CreationTimeUtc.Ticks;
            }

            if (saveLastWriteTime)
            {
                infoRecord.LastWriteTimeUtc = info.LastWriteTimeUtc.ToISOString();
                infoRecord.LastWriteTimeUtcTicks = info.LastWriteTimeUtc.Ticks;
            }

            if (saveLastAccessTime)
            {
                infoRecord.LastAccessTimeUtc = info.LastAccessTimeUtc.ToISOString();
                infoRecord.LastAccessTimeUtcTicks = info.LastAccessTimeUtc.Ticks;
            }

            if (saveSize)
            {
                FileInfo file = (info as FileInfo)!;
                FileInfoRecord fileInfoRecord = (infoRecord as FileInfoRecord)!;
                fileInfoRecord.Size = file.Length;
            }

            if (saveHash)
            {
                FileInfo file = (info as FileInfo)!;
                FileInfoRecord fileInfoRecord = (infoRecord as FileInfoRecord)!;

                Console.WriteLine($"Hash {file.GetRelativePath(dirPath)}");
                ProgressPrinter progressPrinter = new("{0} ({1} / {2}), {3}/s");
                fileInfoRecord.SHA512 = HashComputer.ComputeHash(file.FullName, hashProgress =>
                {
                    progressPrinter.Update(hashProgress.Percentage,
                            hashProgress.TotalUpdatedLength.ToByteString(),
                            hashProgress.TotalLength.ToByteString(),
                            hashProgress.LengthPerSecond);
                });
                progressPrinter.End();
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
            if (infoRecord.CreationTimeUtc != null)
            {
                Console.WriteLine($"  date created: {infoRecord.CreationTimeUtc}");
            }
            if (infoRecord.LastWriteTimeUtc != null)
            {
                Console.WriteLine($"  date modified: {infoRecord.LastWriteTimeUtc}");
            }
            if (infoRecord.LastAccessTimeUtc != null)
            {
                Console.WriteLine($"  date accessed: {infoRecord.LastAccessTimeUtc}");
            }
            if (infoRecord is FileInfoRecord fileInfoRecord)
            {
                if (fileInfoRecord.Size != null)
                {
                    Console.WriteLine($"  size: {fileInfoRecord.Size?.ToByteDetailString()}");
                }
                if (fileInfoRecord.SHA512 != null)
                {
                    Console.WriteLine($"  SHA512: {fileInfoRecord.SHA512}");
                }
            }
        }
    }
}
