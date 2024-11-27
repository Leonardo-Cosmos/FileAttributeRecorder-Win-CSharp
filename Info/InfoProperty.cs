/* 2023/11/7 */

namespace FileInfoTool.Info
{
    /// <summary>
    /// The value indicating a property of file or directory.
    /// </summary>
    internal enum InfoProperty
    {
        /// <summary>
        /// Date created
        /// </summary>
        CreationTime,

        /// <summary>
        /// Date modified
        /// </summary>
        LastWriteTime,

        /// <summary>
        /// Date accessed
        /// </summary>
        LastAccessTime,

        /// <summary>
        /// File size
        /// </summary>
        Size,

        /// <summary>
        /// Hash of file content
        /// </summary>
        Hash,
    }

    internal static class InfoProperties
    {
        internal static readonly InfoProperty[] ValidSaveFileProperties =
        [
            InfoProperty.CreationTime,
            InfoProperty.LastWriteTime,
            InfoProperty.LastAccessTime,
            InfoProperty.Size,
            InfoProperty.Hash,
        ];

        internal static readonly InfoProperty[] ValidValidateFileProperties =
        [
            InfoProperty.CreationTime,
            InfoProperty.LastWriteTime,
            InfoProperty.LastAccessTime,
            InfoProperty.Size,
            InfoProperty.Hash,
        ];

        internal static readonly InfoProperty[] ValidRestoreFileProperties =
        [
            InfoProperty.CreationTime,
            InfoProperty.LastWriteTime,
            InfoProperty.LastAccessTime,
        ];

        internal static readonly InfoProperty[] ValidDirProperties =
        [
            InfoProperty.CreationTime,
            InfoProperty.LastWriteTime,
            InfoProperty.LastAccessTime,
        ];
    }

    internal static class InfoPropertyExtension
    {
        internal static string ToNameString(this InfoProperty propertyName)
        {
            string name = propertyName switch
            {
                InfoProperty.CreationTime => "Date created",
                InfoProperty.LastWriteTime => "Date modified",
                InfoProperty.LastAccessTime => "Date accessed",
                InfoProperty.Size => "Size",
                InfoProperty.Hash => "Hash",
                _ => "Unknown",
            };
            return name;
        }
    }
}
