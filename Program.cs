/* 2023/10/26 */
using FileInfoTool.Info;

Console.WriteLine($"[{string.Join(", ", args)}]");

(string dirPath, string? operation, bool? recursive) = ConsoleArgsParser.ParseArgs(args);

if (operation == null)
{
    Console.WriteLine("No operation specified.");
    return;
}

var infoTool = new FileInfoTool.Info.FileInfoTool(dirPath, recursive: recursive ?? false);
switch (operation)
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
        Console.WriteLine("No valid operation specified.");
        break;
}
