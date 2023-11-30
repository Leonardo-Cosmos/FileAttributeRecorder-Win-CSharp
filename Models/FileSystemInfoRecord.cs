/* 2023/10/27 */

using System.Text.Json.Serialization;

namespace FileInfoTool.Models
{
    internal abstract class FileSystemInfoRecord
    {
        /*
         * Name is mandatory when save, but it cannot be declared as required
         * in order to support initialization in generic method.
         */
        public string? Name { get; set; }

        public string? CreationTimeUtc { get; set; }

        public long? CreationTimeUtcTicks { get; set; }

        public string? LastWriteTimeUtc { get; set; }

        public long? LastWriteTimeUtcTicks { get; set; }

        public string? LastAccessTimeUtc { get; set; }

        public long? LastAccessTimeUtcTicks { get; set; }

        [JsonIgnore]
        public DirectoryInfoRecord? Directory { get; set; }

        public virtual string GetRelativePath()
        {
            List<string> pathtNames = new();

            if (Name == null)
            {
                throw new ArgumentException("Info name is missing");
            }
            pathtNames.Add(Name);

            DirectoryInfoRecord? parent = Directory;
            while (parent != null)
            {
                if (parent.Name == null)
                {
                    throw new ArgumentException("Directory name is missing.");
                }
                pathtNames.Add(parent.Name);

                parent = parent.Directory;
            }

            pathtNames.Reverse();
            var baseDirectoryName = pathtNames[0];
            var path = Path.Combine(pathtNames.ToArray());
            return path[baseDirectoryName.Length..^0];
        }

        public static int CompareByName(FileSystemInfoRecord? x, FileSystemInfoRecord? y)
        {
            if (x?.Name == null)
            {
                if (y?.Name == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (y?.Name == null)
                {
                    return 1;
                }
                else
                {
                    return x.Name.CompareTo(y.Name);
                }
            }
        }
    }
}
