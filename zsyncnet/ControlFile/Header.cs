using System;
using System.IO;

namespace zsyncnet.ControlFile
{
    public class Header
    {
        private string Version { get; }
        private string Filename { get; }
        private DateTime MTime { get; }
        private int Blocksize { get; }
        private long Length { get; }
        private int ChecksumBytes { get; }
        private Boolean SequenceMatches { get; }
        private string Url { get; }
        private string Sha1 { get; }

        /// <summary>
        /// Creates new control file
        /// </summary>
        /// <param name="version"></param>
        /// <param name="filename"></param>
        /// <param name="mTime"></param>
        /// <param name="blocksize"></param>
        /// <param name="length"></param>
        /// <param name="checksumBytes"></param>
        /// <param name="sequenceMatches"></param>
        /// <param name="url"></param>
        /// <param name="sha1"></param>
        public Header(string version, string filename, DateTime mTime, int blocksize, long length, int checksumBytes, bool sequenceMatches, string url, string sha1)
        {
            Version = version;
            Filename = filename;
            MTime = mTime;
            Blocksize = blocksize;
            Length = length;
            ChecksumBytes = checksumBytes;
            SequenceMatches = sequenceMatches;
            Url = url;
            Sha1 = sha1;
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