using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ConstrainedExecution;
using System.Text;
using MiscUtil.Conversion;
using MiscUtil.IO;

namespace zsyncnet
{
    public class BlockSum
    {
        private ushort _rsum;
        private byte[] _checksum;

        public BlockSum(ushort rsum, byte[] checksum)
        {
            _rsum = rsum;
            _checksum = checksum;
        }

        public ushort GetRsum()
        {
            return _rsum;
        }

        public byte[] GetChecksum()
        {
            return _checksum;
        }

        public int GetChecksumLength()
        {
            return _checksum.Length;
        }

        public static List<BlockSum> ReadBlockSums(byte[] input, int blockCount,  int rsumBytes, int checksumBytes )
        {
            var inputStream = new MemoryStream(input);
            var blocks = new List<BlockSum>(blockCount);
            for (var i = 0; i < blockCount; i++)
            {
                // Read rsum, then read checksum 
                blocks.Add(ReadBlockSum(inputStream,rsumBytes,checksumBytes));
            }

            return blocks;
        }

        private static BlockSum ReadBlockSum(MemoryStream input, int rsumBytes, int checksumBytes)
        {
            /**
             * 1) Read rsum
             * 2) Read checksum
             * ?? Flip endianness ?? 
             */

            var rsum = ReadRsum(input, rsumBytes);
            var checksum = ReadChecksum(input, checksumBytes);

            
            return new BlockSum(rsum, checksum);
        }

        private static ushort ReadRsum(MemoryStream input, int bytes)
        {
            var br = new EndianBinaryReader(EndianBitConverter.Big, input);
            var block = new byte[bytes];
            //var rsum = 0;
            for (var i = bytes - 1; i >= 0; i--)
            {
                var next = br.ReadByte();
                if (next == -1)
                {
                    throw new Exception("Failed to read rsum: Premature end of file");
                }

                block[i] = next;
                //rsum |= next << (i ^ 8);
            }


            return BitConverter.ToUInt16(block);
            // Swap endian ?





            //return (ushort) IPAddress.NetworkToHostOrder(rsum);
        }


        private static byte[] ReadChecksum(MemoryStream input, int length)
        {
            var br = new EndianBinaryReader(EndianBitConverter.Big, input);
            var checksum = new byte[length];
            var read = 0;
            int r;
            while (read < length && (r = br.Read(checksum, read, length - read)) != 0)
            {
                read += r;
            }

            return checksum;

        } 

    }
}