using FileInfoTool.Models;
/* 2023/10/26 */

namespace FileInfoTool.Info
{
    enum Mode
    {
        Save,
        Restore,
        Validate,
    }

    internal record LaunchOption(Mode Mode, string DirPath, string? InputFile, string? OutputFile, bool Recursive);

    internal static class ConsoleArgsParser
    {
        private static readonly string[] dirPathKeys = new string[] { "-d", "-dir" };

        private static readonly string[] inputFilePathKeys = new string[] { "-i", "-input" };

        private static readonly string[] outputFilePathKeys = new string[] { "-o", "-output" };

        private static readonly string[] recursiveKeys = new string[] { "-r", "-recursive" };

        /// <summary>
        /// Parses launch option from commande line arguments.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static LaunchOption ParseArgs(string[] args)
        {
            Mode? mode;
            if (args.Length > 0 && !args[0].StartsWith('-'))
            {
                var modeArg = args[0];

                mode = null;
                foreach (var modeValue in Enum.GetValues(typeof(Mode)))
                {
                    var modeName = Enum.GetName(typeof(Mode), modeValue);
                    if (modeName!.Equals(modeArg, StringComparison.OrdinalIgnoreCase))
                    {
                        mode = modeValue as Mode?;
                        break;
                    }
                }

                if (mode == null)
                {
                    throw new ArgumentException($"Unknown mode: {modeArg}");
                }
            }
            else
            {
                throw new ArgumentException("Mode is not specified.");
            }

            var argDict = ConvertArgsToDict(args);

            string? dirPath = FindArgValue(argDict, dirPathKeys);
            DirectoryInfo dir;
            if (dirPath == null)
            {
                // Use working directory by default.
                dir = new DirectoryInfo(".");
                dirPath = dir.FullName;
            }
            else
            {
                dir = new DirectoryInfo(dirPath);
                // Make sure the directory path is absolute.
                dirPath = dir.FullName;
            }

            if (dir.Parent == null)
            {
                throw new ArgumentException("Cannot execute against a root path.");
            }

            string GetDefaultInfoFilePath()
            {
                var inputFileName = string.Format(InfoRecord.DefaultFileNameFormat, dir.Name);
                return Path.Combine(dir.Parent.FullName, inputFileName);
            }

            string? inputFilePath = FindArgValue(argDict, inputFilePathKeys);
            if (inputFilePath == null && (mode == Mode.Restore || mode == Mode.Validate))
            {
                inputFilePath = GetDefaultInfoFilePath();
            }

            string? outputFilePath = FindArgValue(argDict, outputFilePathKeys);
            if (outputFilePath == null && mode == Mode.Save)
            {
                outputFilePath = GetDefaultInfoFilePath();
            }

            bool recursive = false;
            var foundRecursiveKey = Array.Find(recursiveKeys, argDict.ContainsKey);
            if (foundRecursiveKey != null)
            {
                recursive = true;
            }

            return new LaunchOption(mode.Value, dirPath, inputFilePath, outputFilePath, recursive);
        }

        private static Dictionary<string, string> ConvertArgsToDict(string[] args)
        {
            return args.Aggregate(new Dictionary<string, string>(), (dict, arg) =>
            {
                if (arg.StartsWith('-'))
                {
                    var values = arg.Split('=');
                    if (values.Length > 1)
                    {
                        // Argument of key value pair.
                        dict.Add(values[0], values[1]);
                    }
                    else
                    {
                        // Argument of key only.
                        dict.Add(values[0], "");
                    }
                }
                return dict;
            });
        }

        private static string? FindArgValue(Dictionary<string, string> argDict, string[] argKeys)
        {
            var firstFoundKey = Array.Find(argKeys, argDict.ContainsKey);
            if (firstFoundKey != null)
            {
                return argDict[firstFoundKey];
            }
            else
            {
                return null;
            }
        }
    }
}
