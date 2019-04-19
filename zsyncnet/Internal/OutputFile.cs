using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks.Dataflow;
using DotNet.Collections.Generic;

namespace zsyncnet.Internal
{
    
    public class OutputFile
    {
        private enum ChangeType
        {
            Update,
            Remove
        }

        public FileInfo FilePath { get; }
        public FileInfo TempPath { get; }

        public long TotalBytesDownloaded { get; set; }
        private int _blockSize;
        private int _lastBlockSize;
        private long _length;
        private string _sha1;
        private DateTime _mtime;
        private List<BlockSum> _localBlockSums;
        private List<BlockSum> _remoteBlockSums;

        private FileStream _tmpStream;
        private FileStream _existingStream;

        private Uri _fileUri;
        
        private static HttpClient _client = new HttpClient();


        public OutputFile(FileInfo path, zsyncnet.ControlFile cf, Uri fileUri)
        {
            FilePath = path;

            _fileUri = fileUri;
            _blockSize = cf.GetHeader().Blocksize;
            _length = cf.GetHeader().Length;
            _lastBlockSize = (int) (_length % _blockSize == 0 ? _blockSize : _length % _blockSize);
            _sha1 = cf.GetHeader().Sha1;
            _mtime = cf.GetHeader().MTime;
            
            TempPath = new FileInfo(FilePath.FullName + ".part");
            
            // Create all directories 
            Directory.CreateDirectory(TempPath.Directory.FullName);
            
            // Open stream

            _tmpStream = new FileStream(TempPath.FullName, FileMode.Create, FileAccess.ReadWrite);
            _existingStream = new FileStream(FilePath.FullName,FileMode.OpenOrCreate,FileAccess.ReadWrite);

            _tmpStream.SetLength(_length);
            
            _localBlockSums = cf.GetBlockSums();
            var fileBuffer = _existingStream.ToByteArray();
            _existingStream.Position = 0;
            _remoteBlockSums = BlockSum.GenerateBlocksum(fileBuffer,
                cf.GetHeader().WeakChecksumLength, cf.GetHeader().StrongChecksumLength, cf.GetHeader().Blocksize);
            TotalBytesDownloaded = 0;

        }

        public void Patch()
        {
            int bufferSize = 1024 * 1024;
            // Copy existing file, up to new file length 
            var bytesRead = -1;
            var bytes = new byte[bufferSize];

            while ((bytesRead = _existingStream.Read(bytes, 0, bufferSize)) > 0)
            {
                _tmpStream.Write(bytes, 0 , bytesRead);
            }
            
            _existingStream.Close();
            
            // Fetch and Patch

            var delta = BuildDelta();
            foreach (var x in delta)
            {
                if (x.Value != ChangeType.Update) continue;
                var range = GetRange(x.Key);

                Console.WriteLine(range.ToString());
                var req = new HttpRequestMessage
                {
                    RequestUri = _fileUri,
                    Headers = {Range = range}
                };

                var response = _client.SendAsync(req).Result;
                if (response.IsSuccessStatusCode)
                {
                    var content = response.Content.ReadAsByteArrayAsync().Result;
                    TotalBytesDownloaded += content.Length;
                    var offset = x.Key * _blockSize;
                    var length = _blockSize;
                    if (offset + _blockSize > _length)
                    {
                        length = _lastBlockSize;
                    }

                    _tmpStream.Position = offset;
                    _tmpStream.Write(content,0, length);
                }

            }
            
            _tmpStream.Flush();
            _tmpStream.Close();
            

        }

        private RangeHeaderValue GetRange(int block)
        {
            long offset = block * _blockSize;
            return new RangeHeaderValue(offset, offset + _blockSize - 1);
        }
        
        private Dictionary<int, ChangeType> BuildDelta()
        {
            // Create list of changes that need to be made 
                
            /**
             * Logically:
             * If seed file > source file, trim to seed file size
             * If source file > seed file, request all blocks extra
             * Find changed blocks
             */

            
            // TODO: This doesnt actually work (shocker) 
            
            var delta = new Dictionary<int, ChangeType>(); 
            if (_localBlockSums.Count > _remoteBlockSums.Count)
            {
                // We have more data than the new file 
                for (var i = _remoteBlockSums.Count; i < _localBlockSums.Count; i++)
                {
                    delta.Add(i,ChangeType.Remove);
                }
                    
                // Compare the blocks

                for (var i = 0; i < _remoteBlockSums.Count; i++)
                {
                    if (!_localBlockSums.Contains(_remoteBlockSums[i]))
                    {
                        delta.Add(i, ChangeType.Update);
                    }
                }
                    
            }
            else if (_localBlockSums.Count < _remoteBlockSums.Count)
            {
                // Source has more content 
                for (var i = _localBlockSums.Count; i < _remoteBlockSums.Count; i++)
                {
                    if (!_remoteBlockSums.Contains(_localBlockSums[i]))
                    {
                        delta.Add(i, ChangeType.Update);
                    }
                }
                    
                for (var i = 0; i < _localBlockSums.Count; i++)
                {
                    if (!_remoteBlockSums.Contains(_localBlockSums[i]))
                    {
                        delta.Add(i, ChangeType.Update);
                    }
                }
            }
            else
            {
                // Same number of blocks, easy
                for (var i = 0; i < _localBlockSums.Count; i++)
                {
                    if (_localBlockSums[i].GetRsum() != _remoteBlockSums[i].GetRsum())
                    {
                        delta.Add(i, ChangeType.Update);
                    }
                    /*if (!_remoteBlockSums.Contains(_localBlockSums[i]))
                    {
                        delta.Add(i, ChangeType.Update);
                    }*/
                }
            }

            return delta;
        }

    }
}