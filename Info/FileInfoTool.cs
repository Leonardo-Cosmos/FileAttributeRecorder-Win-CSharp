/* 2020/3/13 */

namespace FileInfoTool.Info
{
    internal static class FileInfoTool
    {
        public static void Save(LaunchOption option)
        {
            new InfoSaver(option.DirPath, option.OutputFile!,
                option.FilePropertyNames, option.DirPropertyNames).Save(option.Recursive);
        }

        public static void Restore(LaunchOption option)
        {
            new InfoLoader(option.DirPath, option.InputFile!, true,
                option.FilePropertyNames, option.DirPropertyNames).Load(option.Recursive);
        }

        public static void Validate(LaunchOption option)
        {
            new InfoLoader(option.DirPath, option.InputFile!, false,
                option.FilePropertyNames, option.DirPropertyNames).Load(option.Recursive);
        }
    }
}
