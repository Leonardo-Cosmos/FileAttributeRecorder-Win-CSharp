/* 2020/3/13 */

namespace FileInfoTool.Info
{
    internal class FileInfoTool
    {
        private readonly string dirPath;

        private readonly string infoFilePath;

        private readonly bool recursive;

        public FileInfoTool(string dirPath, string infoFilePath, bool recursive = false)
        {
            this.dirPath = dirPath;
            this.infoFilePath = infoFilePath;
            this.recursive = recursive;
        }

        public void Save()
        {
            new InfoSaver(dirPath, infoFilePath).Save(recursive);
        }

        public void Restore()
        {
            new InfoLoader(dirPath, infoFilePath).Load(recursive, true);
        }

        public void Validate()
        {
            new InfoLoader(dirPath, infoFilePath).Load(recursive, false);
        }
    }
}
