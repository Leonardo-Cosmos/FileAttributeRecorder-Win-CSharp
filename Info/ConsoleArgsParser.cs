/* 2023/10/26 */
using FileInfoTool.Models;

namespace FileInfoTool.Info
{
    enum Mode
    {
        Save,
        Restore,
        Validate,
        ExtractSub,
        AddSub,
        RemoveSub,
    }

    internal record LaunchOption(Mode Mode, string DirPath, string? InputFile, string? OutputFile,
        InfoProperty[]? FilePropertyNames, InfoProperty[]? DirPropertyNames, bool Recursive,
        string? BaseFile, string? RelativePath, string? SubFile, bool Overwrite);

    internal static class ConsoleArgsParser
    {
        private static readonly string[] dirPathKeys = new string[] { "-d", "-dir" };

        private static readonly string[] inputFilePathKeys = new string[] { "-i", "-input" };

        private static readonly string[] outputFilePathKeys = new string[] { "-o", "-output" };

        private static readonly string[] recursiveKeys = new string[] { "-r", "-recursive" };

        private static readonly string[] propertyKeys = new string[] { "-prop", "-property" };

        private static readonly string[] filePropertyKeys = new string[] { "-fprop", "-file-prop", "-file-property" };

        private static readonly string[] dirPropertyKeys = new string[] { "-dprop", "-dir-prop", "-dir-property" };

        private static readonly string[] baseFilePathKeys = new string[] { "-base", "-base-info" };

        private static readonly string[] relativePathKeys = new string[] { "-path", "-relative-path" };

        private static readonly string[] subFilePathKeys = new string[] { "-sub", "-sub-info" };

        private static readonly string[] overwriteKeys = new string[] { "-ow", "-over-write" };

        private const string creationTimePropertyValue = "c";

        private const string lastWriteTimePropertyValue = "m";

        private const string lastAccessPropertyValue = "a";

        private const string sizePropertyValue = "s";

        private const string hashPropertyValue = "h";

        private const string wildcardPropertyValue = "*";

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

            // Convert arguments to key value pairs, skipping the mode argument.
            var argDict = ConvertArgsToDict(args.Skip(1));

            string? dirPath = TakeArgValue(argDict, dirPathKeys);
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

            string? inputFilePath = TakeArgValue(argDict, inputFilePathKeys);
            if (inputFilePath == null && (mode == Mode.Restore || mode == Mode.Validate))
            {
                inputFilePath = GetDefaultInfoFilePath();
            }

            string? outputFilePath = TakeArgValue(argDict, outputFilePathKeys);
            if (outputFilePath == null && mode == Mode.Save)
            {
                outputFilePath = GetDefaultInfoFilePath();
            }

            var recursive = TakeArgValue(argDict, recursiveKeys) != null;

            string? propertyValue = TakeArgValue(argDict, propertyKeys);
            string? filePropertyValue = TakeArgValue(argDict, filePropertyKeys);
            string? dirPropertyValue = TakeArgValue(argDict, dirPropertyKeys);
            if (propertyValue != null)
            {
                filePropertyValue ??= propertyValue;
                dirPropertyValue ??= propertyValue;
            }

            InfoProperty[]? fileProperties;
            if (filePropertyValue != null)
            {
                fileProperties = ParsePropertyValue(filePropertyValue);
            }
            else
            {
                fileProperties = null;
            }

            InfoProperty[]? dirProperties;
            if (dirPropertyValue != null)
            {
                dirProperties = ParsePropertyValue(dirPropertyValue);
            }
            else
            {
                dirProperties = null;
            }

            string? baseFilePath = TakeArgValue(argDict, baseFilePathKeys);

            string? relativePath = TakeArgValue(argDict, relativePathKeys);

            string? subFilePath = TakeArgValue(argDict, subFilePathKeys);

            var overwrite = TakeArgValue(argDict, overwriteKeys) != null;

            if (argDict.Count > 0)
            {
                var unknownArgs = ConvertDictToArgs(argDict);
                Console.WriteLine($"Unknown arguments: {string.Join(" ", unknownArgs)}");
            }

            return new LaunchOption(mode.Value, dirPath, inputFilePath, outputFilePath,
                fileProperties, dirProperties, recursive,
                baseFilePath, relativePath, subFilePath, overwrite);
        }

        private static Dictionary<string, string> ConvertArgsToDict(IEnumerable<string> args)
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

        private static IEnumerable<string> ConvertDictToArgs(Dictionary<string, string> argDict)
        {
            return argDict.Keys
                .Select(key => $"{key}={argDict[key]}")
                .ToArray();
        }

        private static string? TakeArgValue(Dictionary<string, string> argDict, string[] argKeys)
        {
            var firstFoundKey = Array.Find(argKeys, argDict.ContainsKey);
            if (firstFoundKey != null)
            {
                var argValue = argDict[firstFoundKey];
                argDict.Remove(firstFoundKey);
                return argValue;
            }
            else
            {
                return null;
            }
        }

        private static InfoProperty[] ParsePropertyValue(string propertyValue)
        {
            if (propertyValue.Contains(wildcardPropertyValue))
            {
                return Enum.GetValues<InfoProperty>();
            }

            List<InfoProperty> propertyNames = new();
            foreach (char valueChar in propertyValue)
            {
                var nameValue = valueChar.ToString().ToLower();
                switch (nameValue)
                {
                    case creationTimePropertyValue:
                        propertyNames.Add(InfoProperty.CreationTime);
                        break;

                    case lastWriteTimePropertyValue:
                        propertyNames.Add(InfoProperty.LastWriteTime);
                        break;

                    case lastAccessPropertyValue:
                        propertyNames.Add(InfoProperty.LastAccessTime);
                        break;

                    case sizePropertyValue:
                        propertyNames.Add(InfoProperty.Size);
                        break;

                    case hashPropertyValue:
                        propertyNames.Add(InfoProperty.Hash);
                        break;

                    default:
                        break;
                }
            }
            return propertyNames.ToArray();
        }
    }
}
