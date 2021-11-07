using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NLog;

namespace zsyncnet.Internal
{
    public class OutputFile
    {
        private readonly IRangeDownloader _downloader;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly int _blockSize;
        private readonly long _length;
        private readonly List<BlockSum> _localBlockSums;
        private readonly IReadOnlyList<BlockSum> _remoteBlockSums;

        private readonly Stream _tmpStream;
        private readonly Stream _existingStream;
        private readonly string _sha1;


        public OutputFile(Stream input, zsyncnet.ControlFile cf, IRangeDownloader downloader, Stream output)
        {
            _downloader = downloader;

            _blockSize = cf.GetHeader().BlockSize;
            _length = cf.GetHeader().Length;
            _sha1 = cf.GetHeader().Sha1;

            _tmpStream = output ?? new MemoryStream((int)_length);
            _existingStream = input;

            _tmpStream.SetLength(_length);

            _remoteBlockSums = cf.GetBlockSums();
            var fileBuffer = _existingStream.ToByteArray();
            _existingStream.Position = 0;
            _localBlockSums = BlockSum.GenerateBlocksum(fileBuffer,
                cf.GetHeader().WeakChecksumLength, cf.GetHeader().StrongChecksumLength, cf.GetHeader().BlockSize);
            // Set the last mod time to the time in the control file.

        }

        private class DownloadRange
        {
            public long BlockStart, Size;
        }

        private List<DownloadRange> BuildRanges(List<SyncOperation> downloadBlocks)
        {
            // TODO: this is ugly.
            var ranges = new List<DownloadRange>();

            DownloadRange current = null;
            foreach (var downloadBlock in downloadBlocks.Select(block => block.RemoteBlock).OrderBy(block => block.BlockStart))
            {
                if (current == null) // new range
                {
                    current = new DownloadRange
                    {
                        BlockStart = downloadBlock.BlockStart,
                        Size = 1
                    };
                    continue;
                }

                if (downloadBlock.BlockStart == current.BlockStart + current.Size) // append
                {
                    current.Size ++;
                    continue;
                }

                ranges.Add(current);
                current = new DownloadRange
                {
                    BlockStart = downloadBlock.BlockStart,
                    Size = 1
                };
            }
            if (current != null)
                ranges.Add(current);

            return ranges;
        }

        public void Patch()
        {

            _existingStream.CopyTo(_tmpStream);
            _tmpStream.SetLength(_length);
            _existingStream.Close();

            var syncOps = CompareFiles();
            Logger.Info($"Total changed blocks {syncOps.Count}");

            var copyBlocks = syncOps.Where(so => so.LocalBlock != null);
            var downloadBlocks = syncOps.Where(so => so.LocalBlock == null).ToList();

            foreach (var so in copyBlocks)
            {
                // TODO: handle copy blocks
                // for now, just download them as well
                // TODO: benchmark, find out how important they actually are
                downloadBlocks.Add(so);
            }

            var downloadRanges = BuildRanges(downloadBlocks);


            foreach (var op in downloadRanges)
            {
                long offset = op.BlockStart * _blockSize;
                var length = op.Size * _blockSize;
                if (offset + length > _length) // fix size for last block
                {
                    length = _length - offset;
                }

                var content = _downloader.DownloadRange(offset, offset + length);

                _tmpStream.Position = offset;
                content.CopyTo(_tmpStream);
                _tmpStream.Position = 0;
            }

            _tmpStream.Flush();

            if (!VerifyFile(_tmpStream, _sha1))
                throw new Exception("Verification failed");

            _tmpStream.Close();
        }

        private static bool VerifyFile(Stream stream, string checksum)
        {
            stream.Position = 0;
            using var crypto = new SHA1CryptoServiceProvider();
            var hash = ZsyncUtil.ByteToHex(crypto.ComputeHash(stream));
            return hash == checksum;
        }

        private List<SyncOperation> CompareFiles()
        {
            var syncOps = new List<SyncOperation>();

            for (var i = 0; i < _remoteBlockSums.Count; i++)
            {
                var remoteBlock = _remoteBlockSums[i];

                if (i < _localBlockSums.Count && _localBlockSums[i].ChecksumsMatch(remoteBlock)) continue; // present

                var localBlock = _localBlockSums.Find(x => x.ChecksumsMatch(remoteBlock));

                syncOps.Add(new SyncOperation(remoteBlock, localBlock));
            }

            return syncOps;
        }
    }
}
