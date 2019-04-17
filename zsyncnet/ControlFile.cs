using System;
using System.Collections.Generic;
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

        public Header GetHeader()
        {
            return _header;
        }

        public List<BlockSum> GetBlockSums()
        {
            return _blockSums;
        }
        
        
    }
}