using System;
using System.IO;

namespace zsyncnet
{
    public class Rsum
    {
        private uint _bitmask;
        private int _blockShift;
        
        public long A { get; set; }
        public long B { get; set; }
        
        
        public Rsum(int length, int blockSize)
        {
            _bitmask = 4 == length ? 0xffffffff : (uint) (3 == length ? 0xffffff : 2 == length ? 0xffff : 1 == length ? 0xff : 0);
            _blockShift = ComputeBlockShift(blockSize);
            A = 0;
            B = 0;
        }

        public void Initialize(Rsum rsum)
        {
            A = rsum.A;
            B = rsum.B;
        }

        public void Initialize(MemoryStream stream)
        {
            Initialize(stream,0,stream.Length);
        }

        public void Initialize(MemoryStream stream, int offset, long length)
        {
            A = 0;
            B = 0;
            var l = length;
            var buffer = stream.GetBuffer();
            for (var i = 0; i < length; i++, l--)
            {
                var value = UnsignedFromByte(Buffer.GetByte(buffer, i + offset));
                A += value;
                B +=  l * value;
            }
        }
        
        public void Update(byte old, byte newByte)
        {
            A += UnsignedFromByte(newByte) - UnsignedFromByte(old);
            B += A - (UnsignedFromByte(old) << _blockShift);
        }
        
        

        private short UnsignedFromByte(byte b)
        {
            return (short) ((short) b < 0 ? b & 0xFF : b);
        }
        


        private int ComputeBlockShift(int blockSize)
        {
            for (int i = 0; i < 32; i++)
            {
                if (1 << i == blockSize)
                {
                    return i;
                }
            }

            throw new ArgumentException($"Blocksize {blockSize} is not a power of 2");

        }
        
    }
}