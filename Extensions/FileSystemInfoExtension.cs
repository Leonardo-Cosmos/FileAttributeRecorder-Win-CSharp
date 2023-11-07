/* 2023/10/28 */

namespace FileInfoTool.Extensions
{
    internal static class FileSystemInfoExtension
    {
        internal static string GetRelativePath(this FileSystemInfo fileSystemInfo, string basePath)
        {
            string path = fileSystemInfo.FullName;
            if (!path.StartsWith(basePath))
            {
                throw new ArgumentException("The base path is incorrect", nameof(basePath));
            }

            if (fileSystemInfo is DirectoryInfo)
            {
                path += Path.DirectorySeparatorChar;
            }

            return path[basePath.Length..^0];
        }
    }
}
