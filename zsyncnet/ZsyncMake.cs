using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks.Sources;
using zsyncnet.Internal.ControlFile;

namespace zsyncnet
{
    public static class ZsyncMake
    {
        private const int BlockSizeSmall = 2048;
        private const int BlockSizeLarge = 4096;
        private const string ZsyncVersion = "0.6.2";
        public static void Make(FileInfo file)
        {
            
            //var blockSize = 2048;
            var fileLength = file.Length;

            var blockSize = CalculateBlocksize(file.Length);

            var sequenceMatches = fileLength > blockSize ? 2 : 1;
            var weakChecksumLength = CalculateWeakChecksumLength(fileLength, blockSize, sequenceMatches);
            var strongCheckSumLength = CalculateStrongChecksumLength(fileLength, blockSize, sequenceMatches);

            
            var checkSums = ComputeCheckSums(file, blockSize, fileLength, weakChecksumLength, strongCheckSumLength);

            var mtime = File.GetLastWriteTimeUtc(file.FullName);

            var header = new Header(ZsyncVersion, file.Name, mtime, blockSize, fileLength, sequenceMatches,
                weakChecksumLength, strongCheckSumLength, null, ZsyncUtil.ByteToHex(checkSums.sha1));

            var zsyncFile = new FileInfo(file.FullName + ".zsync");
            
            WriteFile(header,new MemoryStream(checkSums.checksums),zsyncFile);
            
            

        }

        /// <summary>
        /// Calculates blocksize based on the zsync paper
        /// </summary>
        /// <param name="fileLength"></param>
        /// <returns></returns>
        private static int CalculateBlocksize(long fileLength)
        {
            return fileLength < 100 * 1 << 20 ? BlockSizeSmall : BlockSizeLarge;
        }

        private static void WriteFile(Header header, MemoryStream checkSums, FileInfo path)
        {
            using (FileStream fs = new FileStream(path.FullName, FileMode.Create, FileAccess.Write))
            {
                fs.Write(StringToBytes(BuildHeaderLine("zsync",header.Version)));
                fs.Write(StringToBytes(BuildHeaderLine("Filename",header.Filename)));
                fs.Write(
                    StringToBytes(BuildHeaderLine("MTime", header.MTime.ToString("r"))));
                fs.Write(StringToBytes(BuildHeaderLine("Blocksize",header.Blocksize.ToString())));
                fs.Write(StringToBytes(BuildHeaderLine("Length",header.Length.ToString())));
                fs.Write(StringToBytes(BuildHeaderLine("Hash-Lengths",$"{header.SequenceMatches},{header.WeakChecksumLength},{header.StrongChecksumLength}")));
                fs.Write(header.Url != null
                    ? StringToBytes(BuildHeaderLine("URL", header.Url))
                    : StringToBytes(BuildHeaderLine("URL", header.Filename)));
                fs.Write(StringToBytes(BuildHeaderLine("SHA-1", header.Sha1)));
                fs.Write(StringToBytes("\n"));

                fs.Write(checkSums.ToArray());
            }
        }

        private static string BuildHeaderLine(string key, string value)
        {
            return $"{key}: {value} \n";
        }

        private static byte[] StringToBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private static int CalculateStrongChecksumLength(long fileLength, int blockSize, int sequenceMatches)
        {
            var d = (Math.Log(fileLength) + Math.Log(1 + fileLength / blockSize)) / Math.Log(2) + 20;

            // reduced number of bits by sequence matches
            var l1 = (int) Math.Ceiling(d / sequenceMatches / 8);

            // second checksum - not reduced by sequence matches
            var l2 = (int) ((Math.Log(1 + fileLength / blockSize) / Math.Log(2) + 20 + 7.9) / 8);

            // return max of two: return no more than 16 bytes (MD4 max)
            return Math.Min(16, Math.Max(l1, l2));
        }

        private static int CalculateWeakChecksumLength(long fileLength, int blockSize, int sequenceMatches)
        {
            double d = (Math.Log(fileLength) + Math.Log(blockSize)) / Math.Log(2) - 8.6;

            // reduced number of bits by sequence matches per http://zsync.moria.org.uk/paper/ch02s04.html
            int l = (int) Math.Ceiling(d / sequenceMatches / 8);

            // enforce max and min values
            return l > 4 ? 4 : l < 2 ? 2 : l;
        }

        private static (byte[] checksums, byte[] sha1) ComputeCheckSums(FileInfo file, int blockSize, long fileLength,
            int weakLength, int strongLength)
        {
            /*
             * As per the zsync spec, a weak checksum is md4 and a strong checksum is sha1
             */

            int capacity = ((int) (fileLength / blockSize) + (fileLength % blockSize > 0 ? 1 : 0)) * (weakLength + strongLength)
                           + 20;

            // 20 = SHA1 length
            
            
            /*
             * CheckSums
             * WeakBytes
             *
             * Limit = Length
             */
            
            var checkSumsMs = new MemoryStream(capacity);
            var weakbytesMs = new MemoryStream(weakLength);

            byte[] block = new byte[blockSize];

            using (BufferedStream stream =
                new BufferedStream(new FileStream(file.FullName,FileMode.Open,FileAccess.Read),1000000))
            {
                int read;

                while ((read = stream.Read(block)) != 0)
                {
                    if (read < blockSize)
                    {
                        // Pad with 0's 
                        block = Pad(block, read, blockSize, 0);
                    }
                    
                    //weakbytesMs.Clear();
                    weakbytesMs.SetLength(0);
                    weakbytesMs.SetLength(weakLength);

                    var weakCheckSum = (ushort) ZsyncUtil.ComputeRsum(block);
                  
                    //weakbytesMs.Position = weakbytesMs.Length - weakLength;
                    
                    checkSumsMs.Write(MiscUtil.Conversion.EndianBitConverter.Big.GetBytes(weakCheckSum));

                    var strongbytesMs = new MemoryStream(ZsyncUtil.Md4Hash(block.ToArray()));
                    strongbytesMs.SetLength(strongLength);
                    
                    byte[] strongBytesBuffer = new byte[strongLength];
                    strongbytesMs.Read(strongBytesBuffer, 0, strongLength);
                    checkSumsMs.Write(strongBytesBuffer,0,strongLength);
                }

                stream.Seek(0, SeekOrigin.Begin);
                var cryptoProvider = new SHA1CryptoServiceProvider();
                var fileHash = cryptoProvider.ComputeHash(stream);
           
                return (checkSumsMs.ToArray(), fileHash);
            }
            
        }
        

        private static byte[] Pad(byte[] array, int start, int end, byte value)
        {
            for (int i = start; i < end; i++)
            {
                array[i] = value;
            }

            return array;
        }
    }
}