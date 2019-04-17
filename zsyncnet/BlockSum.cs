using System.Runtime.ConstrainedExecution;

namespace zsyncnet
{
    public class BlockSum
    {
        private Rsum _rsum;
        private byte[] _checksum;

        public BlockSum(Rsum rsum, byte[] checksum)
        {
            _rsum = rsum;
            _checksum = checksum;
        }

        public Rsum GetRsum()
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
        

    }
}