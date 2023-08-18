using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Base64Tool
{
    public static class Base64Helper
    {
        private const int BucketSize = 0x1000;

        #region Encode
        /// <summary>
        /// Writes encoded contents of source file to target file. If either file name is null, then it reads or
        /// writes from stdin or stdout as appropriate.
        /// </summary>
        /// <param name="sourceFile">The name of the source file, or null to read from stdin.</param>
        /// <param name="targetFile">The name of the target file, or null to write to stdout.</param>
        /// <param name="breakCol">The column number where line breaks should be inserted, or 0 for no line breaks.
        /// </param>
        public static void Encode(string sourceFile, string targetFile, int breakCol = 0)
        {
            using (Stream inStream = OpenInStream(sourceFile), outStream = OpenOutStream(targetFile))
            {
                Encode(inStream, outStream, breakCol);
            }
        }

        /// <summary>
        /// Reads bytes from a stream, and writes them encoded to another stream. 
        /// writes from stdin or stdout as appropriate.
        /// </summary>
        /// <param name="inStream">The stream to read from.</param>
        /// <param name="outStream">The stream to write to.</param>
        /// <param name="breakCol">The column number where line breaks should be inserted, or 0 for no line breaks.</param>
        private static void Encode(Stream inStream, Stream outStream, int breakCol = 0)
        {
            using (var base64Transform = new ToBase64Transform())
            {
                var col = 0;
                var inputBytes = new byte[base64Transform.InputBlockSize * BucketSize];
                var outputBytes = new byte[base64Transform.OutputBlockSize];

                var bytesRead = inStream.Read(inputBytes, 0, inputBytes.Length);
                while (bytesRead != 0)
                {
                    var offset = 0;
                    while (bytesRead - offset > base64Transform.InputBlockSize)
                    {
                        base64Transform.TransformBlock(
                            inputBytes,
                            offset,
                            base64Transform.InputBlockSize,
                            outputBytes,
                            0);
                        col = Emit(outStream, outputBytes, col, breakCol);
                        offset += base64Transform.InputBlockSize;
                    }
                    if (bytesRead - offset > 0)
                    {
                        outputBytes = base64Transform.TransformFinalBlock(
                            inputBytes,
                            offset,
                            bytesRead - offset);
                        Emit(outStream, outputBytes, col, breakCol);
                    }
                    bytesRead = inStream.Read(inputBytes, 0, inputBytes.Length);
                }
            }
        }

        /// <summary>
        /// Writes bytes to a stream, optionally separated into lines.
        /// </summary>
        /// <param name="outStream">The stream to write the bytes to.</param>
        /// <param name="bytes">The bytes to write to stream.</param>
        /// <param name="col">The current column number.</param>
        /// <param name="breakCol">The column number where line breaks should be inserted, or 0 for no line breaks.
        /// </param>
        /// <returns>The updated current column number.</returns>
        private static int Emit(Stream outStream, IEnumerable<byte> bytes, int col, int breakCol)
        {
            var newLine = System.Text.Encoding.ASCII.GetBytes(Environment.NewLine);
            foreach (var byt in bytes)
            {
                if (breakCol != 0 && col++ == breakCol)
                {
                    outStream.Write(newLine, 0, newLine.Length);
                    col = 1;
                }
                outStream.WriteByte(byt);
            }
            return col;
        }
        #endregion Encode

        #region Decode
        /// <summary>
        /// Writes decoded contents of source file to target file. If either file name is null, then it reads or
        /// writes from stdin or stdout as appropriate.
        /// </summary>
        /// <param name="sourceFile">The name of the source file, or null to read from stdin.</param>
        /// <param name="targetFile">The name of the target file, or null to write to stdout.</param>
        public static void Decode(string sourceFile, string targetFile)
        {
            using (Stream inStream = OpenInStream(sourceFile), outStream = OpenOutStream(targetFile))
            {
                Decode(inStream, outStream);
            }
        }

        /// <summary>
        /// Writes decoded contents of source stream to target stream.
        /// </summary>
        /// <param name="inStream">The stream to read from.</param>
        /// <param name="outStream">The stream to write to.</param>
        private static void Decode(Stream inStream, Stream outStream)
        {
            using (var base64Transform = new FromBase64Transform())
            {
                const int inputBlockSize = 4; // base64Transform.InputBlockSize is inexplicably == 1

                var inputBytes = new byte[inputBlockSize * BucketSize];
                var outputBytes = new byte[base64Transform.OutputBlockSize];

                var bytesRead = inStream.Read(inputBytes, 0, inputBytes.Length);
                while (bytesRead != 0)
                {
                    var offset = 0;
                    while (bytesRead - offset > inputBlockSize)
                    {
                        var bytesTransformed = base64Transform.TransformBlock(
                            inputBytes,
                            offset,
                            inputBlockSize,
                            outputBytes,
                            0);
                        outStream.Write(outputBytes, 0, bytesTransformed);
                        offset += inputBlockSize;
                    }
                    if (bytesRead - offset > 0)
                    {
                        outputBytes = base64Transform.TransformFinalBlock(
                            inputBytes,
                            offset,
                            bytesRead - offset);
                        outStream.Write(outputBytes, 0, outputBytes.Length);
                    }
                    bytesRead = inStream.Read(inputBytes, 0, inputBytes.Length);
                }
            }
        }
        #endregion Decode

        #region Private
        /// <summary>
        /// Opens a file or stdout for writing. 
        /// </summary>
        /// <param name="targetFile">The name of the file to open or null to open stdout.</param>
        /// <returns>The output stream.</returns>
        private static Stream OpenOutStream(string targetFile)
        {
            return string.IsNullOrWhiteSpace(targetFile)
                ? Console.OpenStandardOutput()
                : new FileStream(targetFile, FileMode.Create, FileAccess.Write);
        }

        /// <summary>
        /// Opens a file or stdin for reading. 
        /// </summary>
        /// <param name="sourceFile">The name of the file to open or null to open stdin.</param>
        /// <returns>The input stream.</returns>
        private static Stream OpenInStream(string sourceFile)
        {
            return string.IsNullOrWhiteSpace(sourceFile) 
                ? Console.OpenStandardInput()
                : new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
        }
        #endregion Private
    }
}