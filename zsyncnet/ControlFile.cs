using System;
using System.Collections.Generic;
using System.IO;
using zsyncnet.Internal.ControlFile;

namespace zsyncnet
{
    public class ControlFile
    {
        private readonly Header _header;
        private readonly List<BlockSum> _blockSums;

        public ControlFile(Header header, List<BlockSum> blockSums)
        {
            _header = header;
            _blockSums = blockSums;
        }

        public ControlFile(Stream stream)
        {
            // Read stream in (could be from any source)
            var (first, last) = SplitFileRead(stream.ToByteArray());

            _header = new Header(first);
            _blockSums = BlockSum.ReadBlockSums(last, _header.GetNumberOfBlocks(), _header.WeakChecksumLength,
                _header.StrongChecksumLength);
            NLog.LogManager.GetCurrentClassLogger().Info($"Total blocks for {_header.Filename}: {_blockSums.Count}, expected {_header.GetNumberOfBlocks()}");
            if (_header.GetNumberOfBlocks() != _blockSums.Count)
            {
                throw new Exception();
            }
        }

        public Header GetHeader()
        {
            return _header;
        }

        public List<BlockSum> GetBlockSums()
        {
            return _blockSums;
        }

        private static (byte[] first, byte[] last) SplitFileRead(byte[] file)
        {
            var pos = file.Locate(new byte[] {0x0A, 0x0A});

            var offset = pos[0];

            // Two bytes are ignored, they are the two 0x0A's splitting the file

            byte[] first = new byte[offset];
            byte[] last = new byte[file.Length - offset - 2];

            Array.Copy(file, first, offset);
            Array.Copy(file, offset + 2, last, 0, file.Length - offset - 2);

            return (first, last);
        }
    }
}
