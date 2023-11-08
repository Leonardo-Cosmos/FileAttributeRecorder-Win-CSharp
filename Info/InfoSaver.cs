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

        private readonly InfoProperty[] fileProperties;

        private readonly bool saveFileCreationTime;

        private readonly bool saveFileLastWriteTime;

        private readonly bool saveFileLastAccessTime;

        private readonly bool saveFileSize;

        private readonly InfoProperty[] dirProperties;

        private readonly bool saveDirCreationTime;

        private readonly bool saveDirLastWriteTime;

        private readonly bool saveDirLastAccessTime;

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

            saveDirCreationTime = this.dirProperties.Contains(InfoProperty.CreationTime);
            saveDirLastWriteTime = this.dirProperties.Contains(InfoProperty.LastWriteTime);
            saveDirLastAccessTime = this.dirProperties.Contains(InfoProperty.LastAccessTime);
        }

        public void Save(bool recursive)
        {
            Console.WriteLine($"Save file system info, directory: {dirPath}, info file: {infoFilePath}, recursive: {recursive}");
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
                    var subDirInfoRecord = Save(subDirectory, recursive);
                    subDirInfoRecords.Add(subDirInfoRecord);
                }
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
            if (info is FileInfo)
            {
                saveCreationTime = saveFileCreationTime;
                saveLastWriteTime = saveFileLastWriteTime;
                saveLastAccessTime = saveFileLastAccessTime;
                saveSize = saveFileSize;
            }
            else if (info is DirectoryInfo)
            {
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
                var file = info as FileInfo;
                var fileInfoRecord = infoRecord as FileInfoRecord;
                fileInfoRecord!.Size = file!.Length;
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
                Console.WriteLine($"  creation time: {infoRecord.CreationTimeUtc}");
            }
            if (infoRecord.LastWriteTimeUtc !=null)
            {
                Console.WriteLine($"  last write time: {infoRecord.LastWriteTimeUtc},");
            }
            if (infoRecord.LastAccessTimeUtc != null)
            {
                Console.WriteLine($"  last access time: {infoRecord.LastAccessTimeUtc}");
            }
            if (infoRecord is FileInfoRecord fileInfoRecord && fileInfoRecord.Size != null)
            {
                Console.WriteLine($"  size: {fileInfoRecord.Size}");
            }
        }
    }
}
