using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using zsyncnet;
using zsyncnet.Internal;

namespace Tests
{
    public class RollingChecksumTests
    {
        [Test]
        public void Test()
        {
            const int blockSize = 2048;
            var data = new byte[4096];
            new Random(123).NextBytes(data);
            var rSum = RollingChecksum.GetRollingChecksum(data, blockSize);

            var fromRolling = rSum.ToList();
            var expected = new List<uint>();

            for (int i = 0; i < data.Length - blockSize + 1; i++)
            {
                var block = new byte[blockSize];
                Array.Copy(data, i, block, 0, blockSize);
                expected.Add(ZsyncUtil.ComputeRsum(block));
            }

            Assert.AreEqual(expected, fromRolling);
        }

    }
}
