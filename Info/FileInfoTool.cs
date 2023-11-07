/* 2020/3/13 */
using FileInfoTool.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileInfoTool.Info
{
    internal static class FileInfoTool
    {
        public static void Save(LaunchOption option)
        {
            new InfoSaver(option.DirPath, option.OutputFile!).Save(option.Recursive);
        }

        public static void Restore(LaunchOption option)
        {
            new InfoLoader(option.DirPath, option.InputFile!).Load(option.Recursive, true);
        }

        public static void Validate(LaunchOption option)
        {
            new InfoLoader(option.DirPath, option.InputFile!).Load(option.Recursive, false);
        }

    }
}
