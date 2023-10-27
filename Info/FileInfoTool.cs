/* 2020/3/13 */

namespace FileInfoTool.Info
{
    internal class FileInfoTool
    {
        private readonly string dirPath;

        private readonly bool recursive;

        public FileInfoTool(string dirPath, bool recursive = false)
        {
            this.dirPath = dirPath;
            this.recursive = recursive;
        }

        public void Save()
        {
            new InfoSaver().Save(dirPath, recursive);
        }

        public void Restore()
        {
            new InfoLoader().Load(dirPath, recursive, true);
        }

        public void Validate()
        {
            new InfoLoader().Load(dirPath, recursive, false);
        }
    }
}
