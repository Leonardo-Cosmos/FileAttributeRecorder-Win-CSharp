/* 2023/11/16 */
using FileInfoTool.Models;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileInfoTool.Info
{
    internal static class InfoSerializer
    {
        private static readonly JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
        };

        internal static void Serialize(InfoRecord infoRecord, string infoFilePath)
        {
            var json = JsonSerializer.Serialize(infoRecord, options: options);

            File.WriteAllText(infoFilePath, json);

            Console.WriteLine();
            Console.WriteLine($"Write to info file: {infoFilePath}");
            Console.WriteLine();
        }

        internal static InfoRecord? Deserialize(string infoFilePath)
        {
            var json = File.ReadAllText(infoFilePath);

            Console.WriteLine();
            Console.WriteLine($"Read from info file: {infoFilePath}");
            Console.WriteLine();

            var infoRecord = JsonSerializer.Deserialize<InfoRecord>(json, options: options);
            return infoRecord;
        }
    }
}
