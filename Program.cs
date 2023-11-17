/* 2023/10/26 */
using FileInfoTool.Info;

// Console.WriteLine($"[{string.Join(", ", args)}]");

LaunchOption option;
try
{
    option = ConsoleArgsParser.ParseArgs(args);
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return;
}

FileInfoTool.Info.FileInfoTool.Launch(option);
