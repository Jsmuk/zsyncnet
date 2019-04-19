using System;
using System.Collections.Generic;
using System.IO;

namespace zsyncnet
{
    public static class ArrayExtensions
    {
        static readonly int [] Empty = new int [0];

        public static int [] Locate (this byte [] self, byte [] candidate)
        {
            if (IsEmptyLocate (self, candidate))
                return Empty;

            var list = new List<int> ();

            for (int i = 0; i < self.Length; i++) {
                if (!IsMatch (self, i, candidate))
                    continue;

                list.Add (i);
            }

            return list.Count == 0 ? Empty : list.ToArray ();
        }

        static bool IsMatch (byte [] array, int position, byte [] candidate)
        {
            if (candidate.Length > (array.Length - position))
                return false;

            for (int i = 0; i < candidate.Length; i++)
                if (array [position + i] != candidate [i])
                    return false;

            return true;
        }

        static bool IsEmptyLocate (byte [] array, byte [] candidate)
        {
            return array == null
                   || candidate == null
                   || array.Length == 0
                   || candidate.Length == 0
                   || candidate.Length > array.Length;
        }
        
        public static byte[] ToByteArray(this Stream stream)
        {
            using(var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
        
        public static void ReadFully(this Stream stream, byte[] buffer)
        {
            int offset = 0;
            int readBytes;
            do
            {
                // If you are using Socket directly instead of a Stream:
                //readBytes = socket.Receive(buffer, offset, buffer.Length - offset,
                //                           SocketFlags.None);

                readBytes = stream.Read(buffer, offset, buffer.Length - offset);
                offset += readBytes;
            } while (readBytes > 0 && offset < buffer.Length);

            if (offset < buffer.Length)
            {
                throw new EndOfStreamException();
            }
        }
    }
}