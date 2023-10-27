/* 2023/10/26 */

namespace FileInfoTool.Info
{
    internal static class ConsoleArgsParser
    {
        private static readonly string[] operationKeys = new string[] { "-o", "-operation" };

        private static readonly string[] recursiveKeys = new string[] { "-r", "-recursive" };

        public static (string, string?, bool?) ParseArgs(string[] args)
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
                if (arg.Contains('='))
                {
                    var values = arg.Split('=');
                    dict.Add(values[0], values[1]);
                }
                return dict;
            });


            string? operation = null;
            foreach (var key in operationKeys)
            {
                if (argDict.ContainsKey(key))
                { 
                    operation = argDict[key];
                    break;
                }
            }

            string? recursiveValue = null;
            foreach (var key in recursiveKeys)
            {
                if (argDict.ContainsKey(key))
                {
                    recursiveValue = argDict[key];
                    break;
                }
            }

            bool? recursive = null;
            if (recursiveValue != null)
            {
                try
                {
                    recursive = bool.Parse(recursiveValue);
                }
                catch
                {
                    // Invalid format.
                }
            }

            return (dirPath, operation, recursive);
        }
    }
}
