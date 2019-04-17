using System;
using System.IO;

namespace zsyncnet.Internal.ControlFile
{
    public class Header
    {
        public string Version { get; }
        public string Filename { get; }
        public DateTime MTime { get; }
        public int Blocksize { get; }
        public long Length { get; }
        public int WeakChecksumLength { get; }
        public int StrongChecksumLength { get; }
        public int SequenceMatches { get; }
        public string Url { get; }
        public string Sha1 { get; }

        /// <summary>
        /// Creates new control file
        /// </summary>
        /// <param name="version"></param>
        /// <param name="filename"></param>
        /// <param name="mTime"></param>
        /// <param name="blocksize"></param>
        /// <param name="length"></param>
        /// <param name="sequenceMatches"></param>
        /// <param name="url"></param>
        /// <param name="sha1"></param>
        public Header(string version, string filename, DateTime mTime, int blocksize, long length, int sequenceMatches, int weakChecksumLength, int strongChecksumLength ,string url, string sha1)
        {
            Version = version;
            Filename = filename;
            MTime = mTime;
            Blocksize = blocksize;
            Length = length;
            SequenceMatches = sequenceMatches;
            Url = url;
            Sha1 = sha1;
            WeakChecksumLength = weakChecksumLength;
            StrongChecksumLength = strongChecksumLength;
        }

        /// <summary>
        /// Reads a header of control file
        /// </summary>
        /// <param name="input"></param>
        public Header(Stream input)
        {
            
        }
    }
}