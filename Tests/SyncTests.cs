using System;
using System.IO;
using NUnit.Framework;
using Tests.Util;
using zsyncnet;

namespace Tests
{
    public class SyncTests : LoggedTest
    {
        private static void DoTest(byte[] seed, byte[] data, int expectedBlockDownloads, int expectedRanges)
        {
            var cf = ZsyncMake.MakeControlFile(new MemoryStream(data), DateTime.Now, "test.bin");
            var downloader = new DummyRangeDownloader(data);

            var output = new MemoryStream(data.Length);
            Zsync.Sync(cf, new MemoryStream(seed), downloader, output);

            Assert.AreEqual(data, output.ToArray());
            Assert.AreEqual(expectedBlockDownloads * cf.GetHeader().BlockSize, downloader.TotalBytesDownloaded);
            Assert.AreEqual(expectedRanges, downloader.RangesDowloaded);
        }

        [Test]
        public void NoChange()
        {
            var random = new Random();

            var data = new byte[2048 * 2048];
            random.NextBytes(data);

            var seed = new byte[data.Length];
            data.CopyTo(seed, 0);

            DoTest(seed, data, 0, 0);
        }

        [Test]
        public void SimpleChange()
        {
            var random = new Random();

            var data = new byte[2048 * 2048];
            random.NextBytes(data);

            var seed = new byte[data.Length];
            data.CopyTo(seed, 0);
            seed[0] += 128;

            DoTest(seed, data, 1, 1);
        }

        [Test]
        public void CompleteChange()
        {
            var random = new Random();

            var data = new byte[2048 * 2048];
            random.NextBytes(data);

            var seed = new byte[data.Length];
            for (var i = 0; i < data.Length; i++)
            {
                seed[i] = (byte)(data[i] + 128);
            }

            DoTest(seed, data, 2048, 1);
        }

        [Test]
        public void LastByteChange()
        {
            var random = new Random();

            var data = new byte[2048];
            random.NextBytes(data);

            var seed = new byte[data.Length];
            data.CopyTo(seed, 0);
            seed[2047] += 128;

            DoTest(seed, data, 1, 1);
        }

        [Test]
        public void AddedByteAtStart()
        {
            var random = new Random();

            var data = new byte[2048 * 2048];
            random.NextBytes(data);

            var seed = new byte[data.Length + 1];
            data.CopyTo(seed, 1);
            seed[0] = (byte)(data[1] + 128);

            DoTest(seed, data, 0, 0);
        }

        [Test]
        public void RemovedByteAtStart()
        {
            var random = new Random();

            var data = new byte[2048 * 2048];
            random.NextBytes(data);

            var seed = new byte[data.Length - 1];
            for (int i = 0; i < data.Length - 1; i++)
            {
                seed[i] = data[i + 1];
            }

            DoTest(seed, data, 1, 1);
        }
    }
}
