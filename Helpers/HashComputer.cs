/* 2023/11/17 */
using System.Security.Cryptography;

namespace FileInfoTool.Helpers
{
    internal static class HashComputer
    {
        /// <summary>
        /// 4KB buffer size.
        /// </summary>
        private const int bufferSize = 0x1000;

        private const int reportLoopCount = 0x400;

        internal static string ComputeHash(string filePath, IProgress<long>? progress = null)
        {
            using var fileStream = File.OpenRead(filePath);

            var hashComputer = SHA512.Create();
            hashComputer.Initialize();

            var buffer = new byte[bufferSize];
            var bufferReadLength = 0;
            var totalReadLength = 0L;
            var loopCount = 0;
            while ((bufferReadLength = fileStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                hashComputer.TransformBlock(buffer, 0, bufferReadLength, null, 0);
                totalReadLength += bufferReadLength;
                loopCount++;

                if (loopCount == reportLoopCount)
                {
                    progress?.Report(totalReadLength);
                    loopCount = 0;
                }
            }
            hashComputer.TransformFinalBlock(buffer, 0, 0);
            var hashBytes = hashComputer.Hash ?? Array.Empty<byte>();

            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}
