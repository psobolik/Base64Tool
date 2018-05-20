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
        // Encode from sourceFile to targetFile. If either are null then use stdin or stdout as appropriate
        public static void Encode(string sourceFile, string targetFile, int breakCol = 0)
        {
            using (var inStream = OpenInStream(sourceFile))
            {
                using (var outStream = OpenOutStream(targetFile))
                {
                    Encode(inStream, outStream, breakCol);
                }
            }
        }

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

        // Encode inStream to outStream
        public static void Encode(Stream inStream, Stream outStream, int breakCol = 0)
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
        #endregion Encode

        #region Decode
        // Decode from sourceFile to targetFile. If either are null then use stdin or stdout as appropriate
        public static void Decode(string sourceFile, string targetFile)
        {
            using (var inStream = OpenInStream(sourceFile))
            {
                using (var outStream = OpenOutStream(targetFile))
                {
                    Decode(inStream, outStream);
                }
            }
        }

        // Decode inStream to outStream
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
        private static Stream OpenOutStream(string targetFile)
        {
            Stream result;
            if (string.IsNullOrWhiteSpace(targetFile))
            {
                result = Console.OpenStandardOutput();
            }
            else
            {
                // The target file doesn't have to exist
                result = new FileStream(targetFile, FileMode.Create, FileAccess.Write);
            }

            return result;
        }

        private static Stream OpenInStream(string sourceFile)
        {
            Stream result;
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                result = Console.OpenStandardInput();
            }
            else
            {
                if (!File.Exists(sourceFile)) throw new FileNotFoundException(string.Format("Could not open file '{0}'", sourceFile));
                result = new FileStream(sourceFile, FileMode.Open, FileAccess.Read);
            }

            return result;
        }
        #endregion Private
    }
}