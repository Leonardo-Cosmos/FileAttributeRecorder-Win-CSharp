﻿/* 2023/11/16 */
using FileInfoTool.Models;
using Newtonsoft.Json;

namespace FileInfoTool.Info
{
    internal static class InfoSerializer
    {
        internal static void Serialize(InfoRecord infoRecord, string infoFilePath)
        {
            string json = JsonConvert.SerializeObject(infoRecord, Formatting.Indented,
                new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });
            File.WriteAllText(infoFilePath, json);
            Console.WriteLine($"Write to info file: {infoFilePath}");
        }

        internal static InfoRecord? Deserialize(string infoFilePath)
        {
            var json = File.ReadAllText(infoFilePath);
            Console.WriteLine($"Read from info file: {infoFilePath}");
            var infoRecord = JsonConvert.DeserializeObject<InfoRecord>(json);
            return infoRecord;
        }
    }
}
