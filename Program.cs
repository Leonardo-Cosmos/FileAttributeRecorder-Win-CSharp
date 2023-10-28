/* 2023/10/26 */
using FileInfoTool.Info;

// Console.WriteLine($"[{string.Join(", ", args)}]");

(string dirPath, string infoFilePath, string? mode, bool? recursive) = ConsoleArgsParser.ParseArgs(args);

if (mode == null)
{
    Console.WriteLine("Mode is not specified.");
    return;
}

var infoTool = new FileInfoTool.Info.FileInfoTool(dirPath, infoFilePath,
    recursive: recursive ?? false);
switch (mode)
{
    case "save":
        infoTool.Save();
        break;
    case "restore":
        infoTool.Restore();
        break;
    case "validate":
        infoTool.Validate();
        break;
    default:
        Console.WriteLine("No valid mode specified.");
        break;
}
