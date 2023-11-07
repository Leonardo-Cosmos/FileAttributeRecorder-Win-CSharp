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

switch (option.Mode)
{
    case Mode.Save:
        FileInfoTool.Info.FileInfoTool.Save(option);
        break;
    case Mode.Restore:
        FileInfoTool.Info.FileInfoTool.Restore(option);
        break;
    case Mode.Validate:
        FileInfoTool.Info.FileInfoTool.Validate(option);
        break;
}
