/* 2023/11/17 */
using FileInfoTool.Extensions;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Security.Cryptography;

namespace FileInfoTool.Helpers
{
    internal static class HashComputer
    {
        /// <summary>
        /// 4KB buffer size.
        /// </summary>
        private const int bufferSize = 0x1000;

        /// <summary>
        /// 2048
        /// </summary>
        private const int minReportLoop = 0x800;

        /// <summary>
        /// 4MB
        /// </summary>
        private const int minReportLength = 0x400000;

        private const int minReportMilliseconds = 200;

        internal static string ComputeHash(string filePath, HashProgressHandler? reportProgress = null)
        {
            using var fileStream = File.OpenRead(filePath);
            var fileLength = fileStream.Length;

            var hashComputer = SHA512.Create();
            hashComputer.Initialize();

            var buffer = new byte[bufferSize];
            var bufferReadLength = 0;
            var totalReadLength = 0L;
            var reportReadLength = 0L;
            var reportLoopCount = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var elapsedMilliseconds = 0L;
            while ((bufferReadLength = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                hashComputer.TransformBlock(buffer, 0, bufferReadLength, null, 0);
                totalReadLength += bufferReadLength;
                reportReadLength += bufferReadLength;
                reportLoopCount++;

                elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                if (reportLoopCount >= minReportLoop && reportReadLength > minReportLength
                    && elapsedMilliseconds > minReportMilliseconds)
                {
                    reportProgress?.Invoke(new(fileLength, totalReadLength, reportReadLength, elapsedMilliseconds));
                    reportReadLength = 0L;
                    reportLoopCount = 0;
                    stopwatch.Restart();
                }
            }
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            stopwatch.Stop();
            reportProgress?.Invoke(new(fileLength, totalReadLength, reportReadLength, elapsedMilliseconds));
            hashComputer.TransformFinalBlock(buffer, 0, 0);

            var hashBytes = hashComputer.Hash ?? Array.Empty<byte>();
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }

        internal static async Task<string> ComputeHashAsync(string filePath, HashProgressHandler? reportProgress = null)
        {
            using var fileStream = File.OpenRead(filePath);
            var fileLength = fileStream.Length;

            var hashComputer = SHA512.Create();
            hashComputer.Initialize();

            var totalReadLength = 0L;
            var reportReadLength = 0L;
            var reportLoopCount = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var elapsedMilliseconds = 0L;

            PipeReader reader = PipeReader.Create(fileStream);
            try
            {
                while (true)
                {
                    ReadResult result = await reader.ReadAsync();
                    var buffer = result.Buffer;

                    foreach (ReadOnlyMemory<byte> segment in buffer)
                    {
                        var byteBuffer = segment.ToArray();
                        hashComputer.TransformBlock(byteBuffer, 0, byteBuffer.Length, null, 0);
                        totalReadLength += byteBuffer.Length;
                        reportReadLength += byteBuffer.Length;                        
                    }
                    reader.AdvanceTo(buffer.End);
                    reportLoopCount++;

                    elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                    if (reportLoopCount >= minReportLoop && reportReadLength > minReportLength
                        && elapsedMilliseconds > minReportMilliseconds)
                    {
                        reportProgress?.Invoke(new(fileLength, totalReadLength, reportReadLength, elapsedMilliseconds));
                        reportReadLength = 0L;
                        reportLoopCount = 0;
                        stopwatch.Restart();
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }

                elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();
                reportProgress?.Invoke(new(fileLength, totalReadLength, reportReadLength, elapsedMilliseconds));
                hashComputer.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                var hashBytes = hashComputer.Hash ?? Array.Empty<byte>();
                return BitConverter.ToString(hashBytes).Replace("-", "");
            } 
            finally
            {
                await reader.CompleteAsync();
            }
        }
    }

    internal record HashProgress(long TotalLength, long TotalUpdatedLength, long UpdatedLength, long ElapsedMilliseconds)
    {
        public string Percentage
        {
            get
            {
                if (TotalLength == 0)
                {
                    return 1.ToString("00%");
                }
                return ((decimal)TotalUpdatedLength / TotalLength).ToString("00%");
            }
        }

        public string LengthPerSecond
        {
            get
            {
                if (ElapsedMilliseconds == 0)
                {
                    return (1000 * UpdatedLength).ToByteString();
                }
                return (1000 * UpdatedLength / ElapsedMilliseconds).ToByteString();
            }
        }
    }

    internal delegate void HashProgressHandler(HashProgress progress);
}
