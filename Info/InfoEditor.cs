/* 2023/11/16 */
using FileInfoTool.Models;

namespace FileInfoTool.Info
{
    internal static class InfoEditor
    {
        private static string[] SplitPath(string path)
        {
            return path.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                            options: StringSplitOptions.RemoveEmptyEntries);
        }

        public static void ExtractSubDirectory(string? baseInfoFilePath, string? relativePath, string? subInfoFilePath, bool overwrite)
        {
            Console.WriteLine($"""
                Extract sub directory info
                    base info file: {baseInfoFilePath}
                    relative path: {relativePath}
                    sub info file: {subInfoFilePath}
                    overwrite: {overwrite}

                """);

            if (string.IsNullOrEmpty(baseInfoFilePath))
            {
                Console.WriteLine($"Base info file path is not specified");
                return;
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                Console.WriteLine("Relative path is not specified");
                return;
            }

            if (string.IsNullOrEmpty(subInfoFilePath))
            {
                Console.WriteLine($"Sub info file path is not specified");
                return;
            }

            if (!File.Exists(baseInfoFilePath))
            {
                Console.WriteLine($"Base info file doesn't exist, path: {baseInfoFilePath}");
                return;
            }

            if (File.Exists(subInfoFilePath) && !overwrite)
            {
                Console.WriteLine($"Sub info file exists already, path: {subInfoFilePath}");
                return;
            }

            var baseInfoRecord = InfoSerializer.Deserialize(baseInfoFilePath);
            if (baseInfoRecord == null)
            {
                Console.WriteLine($"Invalid base info file, path: {baseInfoFilePath}");
                return;
            }

            var pathDirNames = SplitPath(relativePath);
            var subDirInfoRecord = baseInfoRecord.Directory;
            foreach (var pathDirName in pathDirNames)
            {
                subDirInfoRecord = subDirInfoRecord.Directories?.Find(dirRecord => dirRecord.Name == pathDirName);
                if (subDirInfoRecord == null)
                {
                    break;
                }
            }

            if (subDirInfoRecord == null)
            {
                Console.WriteLine($"Relative path doesn't exist in base info file, path: {relativePath}");
                return;
            }

            var subInfoRecord = InfoRecord.Create(subDirInfoRecord);
            InfoSerializer.Serialize(subInfoRecord, subInfoFilePath);
        }

        public static void AddSubDirectory(string? baseInfoFilePath, string? relativePath, string? subInfoFilePath, bool overwrite)
        {
            Console.WriteLine($"""
                Add sub directory info
                    base info file: {baseInfoFilePath}
                    relative path: {relativePath}
                    sub info file: {subInfoFilePath}
                    overwrite: {overwrite}

                """);

            if (string.IsNullOrEmpty(baseInfoFilePath))
            {
                Console.WriteLine($"Base info file path is not specified");
                return;
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                Console.WriteLine("Relative path is not specified");
                return;
            }

            if (string.IsNullOrEmpty(subInfoFilePath))
            {
                Console.WriteLine($"Sub info file path is not specified");
                return;
            }

            if (!File.Exists(baseInfoFilePath))
            {
                Console.WriteLine($"Base info file doesn't exist, path: {baseInfoFilePath}");
                return;
            }

            var baseInfoRecord = InfoSerializer.Deserialize(baseInfoFilePath);
            if (baseInfoRecord == null)
            {
                Console.WriteLine($"Invalid base info file, path: {baseInfoFilePath}");
                return;
            }

            if (!overwrite)
            {
                Console.WriteLine($"Base info file must be overwritten");
                return;
            }

            var pathDirNames = SplitPath(relativePath);
            var subDirName = pathDirNames[^1];
            var parentDirInfoRecord = baseInfoRecord.Directory;
            foreach (var pathDirName in pathDirNames.SkipLast(1))
            {
                parentDirInfoRecord = parentDirInfoRecord.Directories?.Find(dirInfo => dirInfo.Name == pathDirName);
                if (parentDirInfoRecord == null)
                {
                    break;
                }
            }

            if (parentDirInfoRecord == null)
            {
                Console.WriteLine($"Relative path doesn't exist in base info file, path: {relativePath}");
                return;
            }

            parentDirInfoRecord.Directories ??= new List<DirectoryInfoRecord>();
            var subDirExists = parentDirInfoRecord.Directories.Exists(dirRecord => dirRecord.Name == subDirName);
            if (subDirExists)
            {
                Console.WriteLine($"Sub directory exists in relative path already");
                return;
            }

            var subInfoRecord = InfoSerializer.Deserialize(subInfoFilePath);
            if (subInfoRecord == null)
            {
                Console.WriteLine($"Invalid sub info file, path: {subInfoFilePath}");
                return;
            }

            var subDirInfoRecord = subInfoRecord.Directory;
            if (subDirInfoRecord.Name != subDirName)
            {
                subDirInfoRecord.Name = subDirName;
                Console.WriteLine($"Added directory is renamed to {subDirName}");
            }

            // Insert sub dir at proper index, so that the directory order is consistent.
            var index = parentDirInfoRecord.Directories
                .FindIndex(dirRecord => subDirInfoRecord.Name.CompareTo(dirRecord?.Name) < 1);
            if (index == -1)
            {
                parentDirInfoRecord.Directories.Add(subDirInfoRecord);
            }
            else
            {
                parentDirInfoRecord.Directories.Insert(index, subDirInfoRecord);
            }

            InfoRecord.Update(baseInfoRecord);
            InfoSerializer.Serialize(baseInfoRecord, baseInfoFilePath);
        }

        public static void RemoveSubDirectory(string? baseInfoFilePath, string? relativePath, bool overwrite)
        {
            Console.WriteLine($"""
                Remove sub directory info
                    base info file: {baseInfoFilePath}
                    relative path: {relativePath}
                    overwrite: {overwrite}

                """);

            if (string.IsNullOrEmpty(baseInfoFilePath))
            {
                Console.WriteLine($"Base info file path is not specified");
                return;
            }

            if (string.IsNullOrEmpty(relativePath))
            {
                Console.WriteLine("Relative path is not specified");
                return;
            }

            if (!File.Exists(baseInfoFilePath))
            {
                Console.WriteLine($"Base info file doesn't exist, path: {baseInfoFilePath}");
                return;
            }

            var baseInfoRecord = InfoSerializer.Deserialize(baseInfoFilePath);
            if (baseInfoRecord == null)
            {
                Console.WriteLine($"Invalid base info file, path: {baseInfoFilePath}");
                return;
            }

            if (!overwrite)
            {
                Console.WriteLine($"Base info file must be overwritten");
                return;
            }

            var pathDirNames = SplitPath(relativePath);
            var subDirName = pathDirNames[^1];
            var parentDirInfoRecord = baseInfoRecord.Directory;
            foreach (var pathDirName in pathDirNames.SkipLast(1))
            {
                parentDirInfoRecord = parentDirInfoRecord.Directories?.Find(dirInfo => dirInfo.Name == pathDirName);
                if (parentDirInfoRecord == null)
                {
                    break;
                }
            }

            if (parentDirInfoRecord == null)
            {
                Console.WriteLine($"Relative path doesn't exist in base info file, path: {relativePath}");
                return;
            }

            parentDirInfoRecord.Directories ??= new List<DirectoryInfoRecord>();
            var subDirInfoRecord = parentDirInfoRecord.Directories.Find(dirRecord => dirRecord.Name == subDirName);
            if (subDirInfoRecord == null)
            {
                Console.WriteLine($"Sub directory doesn't exist in relative path");
                return;
            }

            parentDirInfoRecord.Directories.Remove(subDirInfoRecord);

            InfoRecord.Update(baseInfoRecord);
            InfoSerializer.Serialize(baseInfoRecord, baseInfoFilePath);
        }
    }
}
