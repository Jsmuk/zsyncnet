using System.IO;
using zsyncnet;

namespace Tests
{
    internal class DummyRangeDownloader : IRangeDownloader
    {
        private readonly byte[] _data;
        public long TotalBytesDownloaded { get; private set; }
        public long RangesDowloaded { get; private set; }

        public DummyRangeDownloader(byte[] data)
        {
            _data = data;
        }

        public Stream DownloadRange(long @from, long to)
        {
            var stream = new MemoryStream(_data, (int)from, (int)(to - from));
            TotalBytesDownloaded += to - from;
            RangesDowloaded++;
            return stream;
        }
    }
}
