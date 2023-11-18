/* 2020/3/13 */

namespace FileInfoTool.Info
{
    internal static class FileInfoTool
    {
        public static void Launch(LaunchOption option)
        {
            switch (option.Mode)
            {
                case Mode.Save:
                    Save(option);
                    break;
                case Mode.Restore:
                    Restore(option);
                    break;
                case Mode.Validate:
                    Validate(option);
                    break;
                case Mode.ExtractSub:
                    ExtractSubDirectory(option);
                    break;
                case Mode.AddSub:
                    AddSubDirectory(option);
                    break;
                case Mode.RemoveSub:
                    RemoveSubDirectory(option);
                    break;
            }
        }

        private static void Save(LaunchOption option)
        {
            new InfoSaver(option.DirPath, option.OutputFile!,
                option.FilePropertyNames, option.DirPropertyNames).Save(option.Recursive, option.Overwrite);
        }

        private static void Restore(LaunchOption option)
        {
            new InfoLoader(option.DirPath, option.InputFile!, true,
                option.FilePropertyNames, option.DirPropertyNames).Load(option.Recursive);
        }

        private static void Validate(LaunchOption option)
        {
            new InfoLoader(option.DirPath, option.InputFile!, false,
                option.FilePropertyNames, option.DirPropertyNames).Load(option.Recursive);
        }

        private static void ExtractSubDirectory(LaunchOption option)
        {
            InfoEditor.ExtractSubDirectory(option.BaseFile, option.RelativePath, option.SubFile, option.Overwrite);
        }

        private static void AddSubDirectory(LaunchOption option)
        {
            InfoEditor.AddSubDirectory(option.BaseFile, option.RelativePath, option.SubFile, option.Overwrite);
        }

        private static void RemoveSubDirectory(LaunchOption option)
        {
            InfoEditor.RemoveSubDirectory(option.BaseFile, option.RelativePath, option.Overwrite);
        }
    }
}
