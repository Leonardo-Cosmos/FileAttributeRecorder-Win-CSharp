/* 2023/10/26 */
using FileAttributeRecorder.Info;

string opearation;
string dirPath;
if (args.Length == 0)
{
    Console.WriteLine("No operation specified.");
    return;
}
else
{
    opearation = args[0];
}

if (args.Length > 1)
{
    dirPath = args[1];
}
else
{
    dirPath = ".";
}

var fileInfoTool = new FileInfoTool(dirPath);
switch (opearation)
{
    case "-s":
        fileInfoTool.SaveFileDate();
        break;
    case "-r":
        fileInfoTool.RestoreFileDate();
        break;
    default:
        Console.WriteLine("No valid operation specified.");
        break;
}
