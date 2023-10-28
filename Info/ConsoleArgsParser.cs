using FileInfoTool.Models;
/* 2023/10/26 */

namespace FileInfoTool.Info
{
    internal static class ConsoleArgsParser
    {
        private static readonly string[] infoFilePathKeys = new string[] { "-f", "-file" };

        private static readonly string[] modeKeys = new string[] { "-m", "-mode" };

        private static readonly string[] recursiveKeys = new string[] { "-r", "-recursive" };

        public static (string, string, string?, bool) ParseArgs(string[] args)
        {
            string dirPath;
            if (args.Length > 1 && !args[0].StartsWith('-'))
            {
                dirPath = args[0];
            }
            else
            {
                dirPath = ".";
            }

            var argDict = args.Aggregate(new Dictionary<string, string>(), (dict, arg) =>
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

            string? mode = null;
            var foundModeKey = Array.Find(modeKeys, argDict.ContainsKey);
            if (foundModeKey != null)
            {
                mode = argDict[foundModeKey];
            }

            bool recursive = false;
            var foundRecursiveKey = Array.Find(recursiveKeys, argDict.ContainsKey);
            if (foundRecursiveKey != null)
            {
                recursive = true;
            }

            string infoFilePath;
            var foundInfoFileKey = Array.Find(infoFilePathKeys, argDict.ContainsKey);
            if (foundInfoFileKey != null)
            {
                infoFilePath = argDict[foundInfoFileKey];
            }
            else
            {
                infoFilePath = Path.Combine(dirPath, InfoRecord.RecordFileName);
            }

            return (dirPath, infoFilePath, mode, recursive);
        }
    }
}
