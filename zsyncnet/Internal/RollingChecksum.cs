using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// TODO: where should this be placed?
[assembly: InternalsVisibleTo("Tests")]

namespace zsyncnet.Internal
{
    internal static class RollingChecksum
    {
        public static IEnumerable<uint> GetRollingChecksum(byte[] array, int blockSize)
        {
            ushort a = 0, b = 0;
            for (int i = 0; i < blockSize; i++)
            {
                a += array[i];
                b += (ushort)((blockSize - i) * array[i]);
            }

            yield return ToInt(a, b);

            for (int i = 0; i < array.Length - blockSize; i++)
            {
                a = (ushort)(a - array[i] + array[i + blockSize]);
                b = (ushort)(b - blockSize * array[i] + a);
                yield return ToInt(a, b);
            }
        }

        private static uint ToInt(ushort a, ushort b)
        {
            // TODO: rsync one is the other way around. not sure what's going on
            return (uint)(a << 16) | b;
        }
    }
}
